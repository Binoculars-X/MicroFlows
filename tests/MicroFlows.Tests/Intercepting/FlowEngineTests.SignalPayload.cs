using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowEngineTests
{
    [Fact]
    public async Task SignalPayload_ShouldBe_DeliveredToFlow()
    {
        var engine = GetEngine();
        var date = new DateTime(1970,12,13);
        var ps = new FlowParams() { ExternalId = "ORDER-123-4" };
        var ctx = await engine.ExecuteFlow(typeof(SampleSignalPayloadWaitingFlow), ps);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        if (ctx.ExecutionResult.ResultState == ResultStateEnum.Fail)
        {
            throw new Exception(ctx.ExecutionResult.ExceptionMessage);
        }

        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        engine = GetEngine();
        var ps2 = new FlowParams() { ExternalId = "ORDER-123-4" };

        var ctx2 = await engine.SendSignal(typeof(SampleSignalPayloadWaitingFlow), SampleSignalPayloadWaitingFlow.Signal1, ps2,
            date);

        var flow2 = await _repo.GetFlowModel(ctx2.RefId);

        Assert.Equal(date, ctx2.Model.Records["$.Signal1PayloadDate"].Deserialize());
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task SignalPayload_Should_WaitForTwoSignals()
    {
        var engine = GetEngine();
        var date = new DateTime(1970, 12, 13);
        var text = "payload text";
        
        var ps = new FlowParams() { ExternalId = "ORDER-123-5" };
        var ctx = await engine.ExecuteFlow(typeof(SampleTwoSignalPayloadWaitingFlow), ps);
        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        // send Signal1
        engine = GetEngine();
        var ps2 = new FlowParams() { ExternalId = "ORDER-123-5" };

        var ctx2 = await engine.SendSignal(typeof(SampleTwoSignalPayloadWaitingFlow), 
            SampleTwoSignalPayloadWaitingFlow.Signal1, ps2, date);

        var flow2 = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(date, ctx2.Model.Records["$.Signal1PayloadDate"].Deserialize());
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx2.ExecutionResult.FlowState);

        // send Signal2
        engine = GetEngine();
        var ps3 = new FlowParams() { ExternalId = "ORDER-123-5" };

        var ctx3 = await engine.SendSignal(typeof(SampleTwoSignalPayloadWaitingFlow),
            SampleTwoSignalPayloadWaitingFlow.Signal2, ps3, text);

        var flow3 = await _repo.GetFlowModel(ctx3.RefId);
        Assert.Equal(date, ctx3.Model.Records["$.Signal1PayloadDate"].Deserialize());
        Assert.Equal(text, ctx3.Model.Records["$.Signal2PayloadText"].Deserialize());
        Assert.Equal(ResultStateEnum.Success, ctx3.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx3.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task SignalPayload_Should_WaitForTwoSignals_ReversedOrder()
    {
        var engine = GetEngine();
        var date = new DateTime(1970, 12, 13);
        var text = "payload text";

        var ps = new FlowParams() { ExternalId = "ORDER-123-6" };
        var ctx = await engine.ExecuteFlow(typeof(SampleTwoSignalPayloadWaitingFlow), ps);
        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        // send Signal2
        // the flow waiting for Signal1, so it will not proceed until receives Signal1
        engine = GetEngine();
        var ps2 = new FlowParams() { ExternalId = "ORDER-123-6" };

        var ctx2 = await engine.SendSignal(typeof(SampleTwoSignalPayloadWaitingFlow),
            SampleTwoSignalPayloadWaitingFlow.Signal2, ps2, text);

        var flow2 = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(4, flow2.ContextHistory.Count);
        Assert.Null(ctx2.Model.Records["$.Signal1PayloadDate"].Deserialize());
        Assert.Null(ctx2.Model.Records["$.Signal2PayloadText"].Deserialize());
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx2.ExecutionResult.FlowState);

        // send Signal1
        engine = GetEngine();
        var ps3 = new FlowParams() { ExternalId = "ORDER-123-6" };

        var ctx3 = await engine.SendSignal(typeof(SampleTwoSignalPayloadWaitingFlow),
            SampleTwoSignalPayloadWaitingFlow.Signal1, ps3, date);

        var flow3 = await _repo.GetFlowModel(ctx3.RefId);
        Assert.Equal(date, ctx3.Model.Records["$.Signal1PayloadDate"].Deserialize());
        Assert.Equal(text, ctx3.Model.Records["$.Signal2PayloadText"].Deserialize());
        Assert.Equal(ResultStateEnum.Success, ctx3.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx3.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task SignalPayload_Should_TriggerSignalHandler_WhenCheckSignalReceived()
    {
        var engine = GetEngine();
        var date = DateTime.UtcNow.Date;
        var ps = new FlowParams() { ExternalId = "ORDER-123-67" };
        var ctx = await engine.ExecuteFlow(typeof(SampleCheckSignalFlow), ps);
        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        // send Cancel, flow should still be blocked by OrderAcceptedSignal 
        engine = GetEngine();

        var ctx2 = await engine.SendSignal(typeof(SampleCheckSignalFlow),
            SampleCheckSignalFlow.OrderCancelledSignal, ps, date);

        var flow2 = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(5, flow2.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx2.ExecutionResult.FlowState);

        // send OrderAcceptedSignal, it should unblock flow and Cancel the order
        engine = GetEngine();
        var ctx3 = await engine.SendSignal(typeof(SampleCheckSignalFlow), SampleCheckSignalFlow.OrderAcceptedSignal, ps);

        var flow3 = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(9, flow3.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx3.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx3.ExecutionResult.FlowState);
        Assert.NotNull(flow3.ContextHistory.Last().Model.Records["$.CancelDate"].Deserialize());
        Assert.Equal("Cancelled", flow3.ContextHistory.Last().Model.Records["$.OrderStatus"].Deserialize());
    }
}
