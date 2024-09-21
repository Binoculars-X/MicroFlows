using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowEngineTests : TestBase
{
    readonly MemoryFlowRepository _repo;

    public FlowEngineTests()
    {
        _repo = new MemoryFlowRepository();
    }

    private FlowEngine GetEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    [Fact]
    public async Task Engine_Should_ThrowException_ForNotRegisteredFlow()
    {
        var engine = GetEngine();
        await Assert.ThrowsAsync<FlowValidationException>(async () => await engine.ExecuteFlow(this.GetType(), null));
    }

    [Fact]
    public async Task SampleFlow_Run_And_Stopped()
    {
        var engine = GetEngine();
        var ps = new FlowParams();
        ps["flag"] = "stop";
        var ctx = await engine.ExecuteFlow(typeof(SampleFlow), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory.Last().ExecutionResult.FlowState);
        Assert.Equal("FlowStopException", flow.ContextHistory.Last().ExecutionResult.ExceptionType);
        //Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory.Last().CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        //Assert.Equal("CallAsync<Flow>b__17_0:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        //Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory[2].CurrentTask);

        // resume
        ps["flag"] = "don't stop";
        ps.RefId = ctx.RefId;
        var ctx2 = await engine.ExecuteFlow(typeof(SampleFlow), ps);
    }

    [Fact]
    public async Task Flow_Stops_When_ConditionFalse()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleWaitingFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Records["$.OrderId"].Deserialize());
        Assert.Equal(false, flow.ContextHistory[0].Model.Records["$.InvoiceSent"].Deserialize());
        Assert.Null(flow.ContextHistory[0].Model.Records["$.SentOrderId"].Deserialize());
        
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_GenerateOrderId:1", flow.ContextHistory[1].CurrentTask);
        Assert.NotNull(flow.ContextHistory[1].Model.Records["$.OrderId"].Deserialize());
        Assert.Equal(false, flow.ContextHistory[1].Model.Records["$.InvoiceSent"].Deserialize());
        Assert.Null(flow.ContextHistory[1].Model.Records["$.SentOrderId"].Deserialize());

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Anonymous:2", flow.ContextHistory[2].CurrentTask);
        Assert.NotNull(flow.ContextHistory[2].Model.Records["$.OrderId"].Deserialize());
        Assert.Equal(false, flow.ContextHistory[2].Model.Records["$.InvoiceSent"].Deserialize());
        Assert.NotNull(flow.ContextHistory[2].Model.Records["$.SentOrderId"].Deserialize());

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[3].ExecutionResult.FlowState);
        Assert.Equal("WaitForCondition:3", flow.ContextHistory[3].CurrentTask);
        Assert.NotNull(flow.ContextHistory[3].Model.Records["$.OrderId"].Deserialize());
        Assert.Equal(false, flow.ContextHistory[3].Model.Records["$.InvoiceSent"].Deserialize());
        Assert.NotNull(flow.ContextHistory[3].Model.Records["$.SentOrderId"]);
    }

    [Fact]
    public async Task SampleLoggingFlow_Not_Logging_WhenResumed()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleLoggingFlow), null);

        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // resume
        engine = GetEngine();
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };
        await engine.ExecuteFlow(typeof(SampleLoggingFlow), ps);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleLoggingFlow_ShouldThrowException_WhenResumedWithWrongHistoryOrCode()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleLoggingFlow), null);
        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // let's corrupt flow history
        var flow = _repo._flowModelDictionary[ctx.RefId];

        // we change CallAsync_GenerateOrderId to CallAsync_Anonymous
        Assert.Equal("CallAsync_GenerateOrderId:1", flow.ContextHistory[1].CurrentTask);
        flow.ContextHistory[1].CurrentTask = "CallAsync_Anonymous:1";
        Assert.Equal("CallAsync_Anonymous:1", flow.ContextHistory[1].CurrentTask);

        // resume and catch exception
        engine = GetEngine();
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };

        var exc = await Assert.ThrowsAsync<NonDeterministicFlowException>(async () => 
            await engine.ExecuteFlow(typeof(SampleLoggingFlow), ps));

        Assert.Contains(ctx.RefId, exc.Message);
        Assert.Contains("CallAsync_GenerateOrderId:1", exc.Message);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleLoggingFlow_ShouldThrowException_WhenResumedWithWrongHistoryOrCode_LastStep()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleLoggingFlow), null);
        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // let's corrupt flow history
        var flow = _repo._flowModelDictionary[ctx.RefId];

        // we change CallAsync_GenerateOrderId to CallAsync_Anonymous
        Assert.Equal("WaitForCondition:4", flow.ContextHistory[4].CurrentTask);
        flow.ContextHistory[4].CurrentTask = "CallAsync_Anonymous:1";
        Assert.Equal("CallAsync_Anonymous:1", flow.ContextHistory[4].CurrentTask);

        // resume and catch exception
        engine = GetEngine();
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };

        var exc = await Assert.ThrowsAsync<NonDeterministicFlowException>(async () =>
            await engine.ExecuteFlow(typeof(SampleLoggingFlow), ps));

        Assert.Contains(ctx.RefId, exc.Message);
        Assert.Contains("WaitForCondition:4", exc.Message);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleExceptionInActionFlow_Should_SaveFailedStep()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleExceptionInActionFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(3, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Records["$.ModelInt"].Deserialize());
        Assert.Null(flow.ContextHistory[0].Model.Records["$.ModelString"].Deserialize());

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(33, flow.ContextHistory[1].Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("test", flow.ContextHistory[1].Model.Records["$.ModelString"].Deserialize());

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, flow.ContextHistory[2].ExecutionResult.ResultState);
        Assert.Equal("Exception", flow.ContextHistory[2].ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync_Anonymous:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(33, flow.ContextHistory[2].Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("test", flow.ContextHistory[2].Model.Records["$.ModelString"].Deserialize());
    }

    [Fact]
    public async Task SampleExceptionFlow_Should_SaveFailedStep()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleExceptionFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Records["$.ModelInt"].Deserialize());
        Assert.Null(flow.ContextHistory[0].Model.Records["$.ModelString"].Deserialize());

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(33, flow.ContextHistory[1].Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("test", flow.ContextHistory[1].Model.Records["$.ModelString"].Deserialize());

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, flow.ContextHistory[2].ExecutionResult.ResultState);
        Assert.Null(flow.ContextHistory[2].ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync_Anonymous:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(33, flow.ContextHistory[2].Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("testtest", flow.ContextHistory[2].Model.Records["$.ModelString"].Deserialize());

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[3].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, flow.ContextHistory[3].ExecutionResult.ResultState);
        Assert.Equal("Exception", flow.ContextHistory[3].ExecutionResult.ExceptionType);
        Assert.Null(flow.ContextHistory[3].CurrentTask);
        Assert.Equal(33, flow.ContextHistory[3].Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("testtest", flow.ContextHistory[3].Model.Records["$.ModelString"].Deserialize());

        // resume
        engine = GetEngine();
        var ps = new FlowParams() { RefId = ctx.RefId };
        await engine.ExecuteFlow(typeof(SampleExceptionFlow), ps);
        flow = await _repo.GetFlowModel(ctx.RefId);

        // should save failed context again
        Assert.Equal(5, flow.ContextHistory.Count);

        var context = flow.ContextHistory[3];
        Assert.Equal(FlowStateEnum.Stop, context.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, context.ExecutionResult.ResultState);
        Assert.Equal("Exception", context.ExecutionResult.ExceptionType);
        Assert.Null(context.CurrentTask);
        Assert.Equal(33, context.Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("testtest", context.Model.Records["$.ModelString"].Deserialize());

        context = flow.ContextHistory[4];
        Assert.Equal(FlowStateEnum.Stop, context.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, context.ExecutionResult.ResultState);
        Assert.Equal("Exception", context.ExecutionResult.ExceptionType);
        Assert.Null(context.CurrentTask);
        Assert.Equal(33, context.Model.Records["$.ModelInt"].Deserialize());
        Assert.Equal("testtest", context.Model.Records["$.ModelString"].Deserialize());
    }
}
