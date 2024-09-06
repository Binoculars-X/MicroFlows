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
    [Fact]
    public async Task Engine_Should_ThrowException_ForNotRegisteredFlow()
    {
        var repo = new MemoryFlowRepository();

        var engine = new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            repo);

        await Assert.ThrowsAsync<FlowValidationException>(async () => await engine.ExecuteFlow(this.GetType(), null));
    }

    [Fact]
    public async Task Test1()
    {
        var repo = new MemoryFlowRepository();

        var engine = new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            repo);

        var ps = new FlowParams();
        ps["flag"] = "stop";
        var ctx = await engine.ExecuteFlow(typeof(SampleFlow), ps);

        var flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory.Last().ExecutionResult.FlowState);
        Assert.Equal("FlowStopException", flow.ContextHistory.Last().ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory.Last().CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync<Flow>b__17_0:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory[2].CurrentTask);

        // resume
        ps["flag"] = "don't stop";
        ps.RefId = ctx.RefId;
        var ctx2 = await engine.ExecuteFlow(typeof(SampleFlow), ps);
    }

    [Fact]
    public async Task Flow_Stops_When_ConditionFalse()
    {
        var repo = new MemoryFlowRepository();

        var engine = new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            repo);

        var ctx = await engine.ExecuteFlow(typeof(SampleWaitingFlow), null);
        var flow = await repo.GetFlowModel(ctx.RefId);

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
}
