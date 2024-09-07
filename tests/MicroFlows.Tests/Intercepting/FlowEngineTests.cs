using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
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

namespace MicroFlows.Tests.Interceptors;

public class FlowEngineTests : TestBase
{
    readonly FlowEngine _engine;
    readonly MemoryFlowRepository _repo;

    public FlowEngineTests()
    {
        _repo = new MemoryFlowRepository();

        _engine = new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    [Fact]
    public async Task Engine_Should_ThrowException_ForNotRegisteredFlow()
    {
        await Assert.ThrowsAsync<FlowValidationException>(async () => await _engine.ExecuteFlow(this.GetType(), null));
    }

    [Fact]
    public async Task Test1()
    {
        var ps = new FlowParams();
        ps["flag"] = "stop";
        var ctx = await _engine.ExecuteFlow(typeof(SampleFlow), ps);

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
        var ctx2 = await _engine.ExecuteFlow(typeof(SampleFlow), ps);
    }

    [Fact]
    public async Task Flow_Stops_When_ConditionFalse()
    {
        var ctx = await _engine.ExecuteFlow(typeof(SampleWaitingFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Null(flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.OrderId"]);
        Assert.Equal("False", flow.ContextHistory[0].Model.Values["$.InvoiceSent"]);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.SentOrderId"]);
        
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_GenerateOrderId:1", flow.ContextHistory[1].CurrentTask);
        Assert.NotNull(flow.ContextHistory[1].Model.Values["$.OrderId"]);
        Assert.Equal("False", flow.ContextHistory[1].Model.Values["$.InvoiceSent"]);
        Assert.Null(flow.ContextHistory[1].Model.Values["$.SentOrderId"]);

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Anonymus:2", flow.ContextHistory[2].CurrentTask);
        Assert.NotNull(flow.ContextHistory[2].Model.Values["$.OrderId"]);
        Assert.Equal("False", flow.ContextHistory[2].Model.Values["$.InvoiceSent"]);
        Assert.NotNull(flow.ContextHistory[2].Model.Values["$.SentOrderId"]);

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[3].ExecutionResult.FlowState);
        Assert.Equal("WaitForCondition:3", flow.ContextHistory[3].CurrentTask);
        Assert.NotNull(flow.ContextHistory[3].Model.Values["$.OrderId"]);
        Assert.Equal("False", flow.ContextHistory[3].Model.Values["$.InvoiceSent"]);
        Assert.NotNull(flow.ContextHistory[3].Model.Values["$.SentOrderId"]);
    }

    [Fact]
    public async Task SampleLoggingFlow_Not_Logging_WhenResumed()
    {         
        var ctx = await _engine.ExecuteFlow(typeof(SampleLoggingFlow), null);

        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // resume
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };
        await _engine.ExecuteFlow(typeof(SampleLoggingFlow), ps);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleLoggingFlow_ShouldThrowException_WhenResumedWithWrongHistoryOrCode()
    {
        var ctx = await _engine.ExecuteFlow(typeof(SampleLoggingFlow), null);
        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // let's corrupt flow history
        var flow = _repo._contextDictHistory[ctx.RefId];

        // we change CallAsync_GenerateOrderId to CallAsync_Anonymus
        Assert.Equal("CallAsync_GenerateOrderId:1", flow.ContextHistory[1].CurrentTask);
        flow.ContextHistory[1].CurrentTask = "CallAsync_Anonymus:1";
        Assert.Equal("CallAsync_Anonymus:1", flow.ContextHistory[1].CurrentTask);

        // resume and catch exception
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };

        var exc = await Assert.ThrowsAsync<NonDeterministicFlowException>(async () => 
            await _engine.ExecuteFlow(typeof(SampleLoggingFlow), ps));

        Assert.Contains(ctx.RefId, exc.Message);
        Assert.Contains("CallAsync_GenerateOrderId:1", exc.Message);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleLoggingFlow_ShouldThrowException_WhenResumedWithWrongHistoryOrCode_LastStep()
    {
        var ctx = await _engine.ExecuteFlow(typeof(SampleLoggingFlow), null);
        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // let's corrupt flow history
        var flow = _repo._contextDictHistory[ctx.RefId];

        // we change CallAsync_GenerateOrderId to CallAsync_Anonymus
        Assert.Equal("WaitForCondition:4", flow.ContextHistory[4].CurrentTask);
        flow.ContextHistory[4].CurrentTask = "CallAsync_Anonymus:1";
        Assert.Equal("CallAsync_Anonymus:1", flow.ContextHistory[4].CurrentTask);

        // resume and catch exception
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };

        var exc = await Assert.ThrowsAsync<NonDeterministicFlowException>(async () =>
            await _engine.ExecuteFlow(typeof(SampleLoggingFlow), ps));

        Assert.Contains(ctx.RefId, exc.Message);
        Assert.Contains("WaitForCondition:4", exc.Message);
        Assert.Empty(SampleLoggingFlow.Log);
    }

    [Fact]
    public async Task SampleExceptionInActionFlow_Should_SaveFailedStep()
    {
        var ctx = await _engine.ExecuteFlow(typeof(SampleExceptionInActionFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(3, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Null(flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.ModelInt"]);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.ModelString"]);

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("33", flow.ContextHistory[1].Model.Values["$.ModelInt"]);
        Assert.Equal("test", flow.ContextHistory[1].Model.Values["$.ModelString"]);

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, flow.ContextHistory[2].ExecutionResult.ResultState);
        Assert.Equal("Exception", flow.ContextHistory[2].ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync_Anonymus:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("33", flow.ContextHistory[2].Model.Values["$.ModelInt"]);
        Assert.Equal("test", flow.ContextHistory[2].Model.Values["$.ModelString"]);
    }

    [Fact]
    public async Task SampleExceptionFlow_Should_SaveFailedStep()
    {
        var ctx = await _engine.ExecuteFlow(typeof(SampleExceptionFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);

        Assert.Equal(FlowStateEnum.Start, flow.ContextHistory[0].ExecutionResult.FlowState);
        Assert.Null(flow.ContextHistory[0].CurrentTask);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.ModelInt"]);
        Assert.Null(flow.ContextHistory[0].Model.Values["$.ModelString"]);

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("33", flow.ContextHistory[1].Model.Values["$.ModelInt"]);
        Assert.Equal("test", flow.ContextHistory[1].Model.Values["$.ModelString"]);

        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, flow.ContextHistory[2].ExecutionResult.ResultState);
        Assert.Null(flow.ContextHistory[2].ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync_Anonymus:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("33", flow.ContextHistory[2].Model.Values["$.ModelInt"]);
        Assert.Equal("testtest", flow.ContextHistory[2].Model.Values["$.ModelString"]);

        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[3].ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, flow.ContextHistory[3].ExecutionResult.ResultState);
        Assert.Equal("Exception", flow.ContextHistory[3].ExecutionResult.ExceptionType);
        Assert.Null(flow.ContextHistory[3].CurrentTask);
        Assert.Equal("33", flow.ContextHistory[3].Model.Values["$.ModelInt"]);
        Assert.Equal("testtest", flow.ContextHistory[3].Model.Values["$.ModelString"]);

        // resume
        var ps = new FlowParams() { RefId = ctx.RefId };
        await _engine.ExecuteFlow(typeof(SampleExceptionFlow), ps);
        flow = await _repo.GetFlowModel(ctx.RefId);

        // should save failed context again
        Assert.Equal(5, flow.ContextHistory.Count);

        var context = flow.ContextHistory[3];
        Assert.Equal(FlowStateEnum.Stop, context.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, context.ExecutionResult.ResultState);
        Assert.Equal("Exception", context.ExecutionResult.ExceptionType);
        Assert.Null(context.CurrentTask);
        Assert.Equal("33", context.Model.Values["$.ModelInt"]);
        Assert.Equal("testtest", context.Model.Values["$.ModelString"]);

        context = flow.ContextHistory[4];
        Assert.Equal(FlowStateEnum.Stop, context.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Fail, context.ExecutionResult.ResultState);
        Assert.Equal("Exception", context.ExecutionResult.ExceptionType);
        Assert.Null(context.CurrentTask);
        Assert.Equal("33", context.Model.Values["$.ModelInt"]);
        Assert.Equal("testtest", context.Model.Values["$.ModelString"]);
    }
}
