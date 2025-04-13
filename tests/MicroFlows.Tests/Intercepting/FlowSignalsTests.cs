using JsonPathToModel;
using MicroFlows.Domain.Enums;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowSignalsTests : TestBase
{
    [Fact]
    public async Task SignalWaitingFlow_Passes_On_Timeout()
    {
        var engine = NewEngine();
        var ps = new FlowParams() { ExternalId = "ORDER-123" };
        var ctx = await engine.ExecuteFlow(typeof(SampleSignalWaitingTimeoutFlow), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal1:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(FlowStateEnum.Waiting, ctx.ExecutionResult.FlowState);

        await Task.Delay(1100);
        engine = NewEngine();
        var ctx2 = await engine.ExecuteFlow(typeof(SampleSignalWaitingTimeoutFlow), ps);

        Assert.Equal(ctx.RefId, ctx2.RefId);

        flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Anonymous:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
        
        var m1 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[1].Model.ExportTo(m1);
        Assert.Null(m1.Timeout1);
        Assert.Null(m1.Timeout2);

        var m2 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[2].Model.ExportTo(m2);
        Assert.Equal(FlowStateEnum.Waiting, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.True(m2.Timeout1);
        Assert.Null(m2.Timeout2);

        var m3 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[3].Model.ExportTo(m3);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[3].ExecutionResult.FlowState);
        Assert.True(m3.Timeout1);
        Assert.Null(m3.Timeout2);

        var m4 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[4].Model.ExportTo(m4);
        Assert.True(m4.Timeout1);
        Assert.True(m4.Timeout2);

        var m5 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[4].Model.ExportTo(m5);
        Assert.True(m5.Timeout1);
        Assert.True(m5.Timeout2);
    }

    [Fact]
    public async Task WaitingFlow_Passes_ToUpdate_On_Signal()
    {
        var engine = NewEngine();
        var ps = new FlowParams() { ExternalId = "ORDER-1234" };
        var ctx = await engine.ExecuteFlow(typeof(SampleWaitingFlow1), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal1:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(FlowStateEnum.Waiting, ctx.ExecutionResult.FlowState);

        var ctx2 = await engine.SendSignal(typeof(SampleWaitingFlow1), SampleWaitingFlow1.Signal2, ps);
        flow = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Update:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task WaitingFlow_Goes_ToCancel_On_Timeout()
    {
        var engine = NewEngine();
        var ps = new FlowParams() { ExternalId = "ORDER-1234" };
        var ctx = await engine.ExecuteFlow(typeof(SampleWaitingFlow1), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal1:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(FlowStateEnum.Waiting, ctx.ExecutionResult.FlowState);

        // provoke timeout
        await Task.Delay(1100);

        var ctx2 = await engine.SendSignal(typeof(SampleWaitingFlow1), SampleWaitingFlow1.Signal2, ps);
        flow = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal2:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Cancel:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task WaitingFlow_Handles_Timeout()
    {
        var engine = NewEngine();
        var ps = new FlowParams() { ExternalId = "ORDER-1234" };
        var ctx = await engine.ExecuteFlow(typeof(SampleHandlingTimeoutFlow1), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(2, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal1:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(FlowStateEnum.Waiting, ctx.ExecutionResult.FlowState);

        // provoke timeout
        await Task.Delay(1100);

        var ctx2 = await engine.SendSignal(typeof(SampleHandlingTimeoutFlow1), SampleHandlingTimeoutFlow1.Signal1, ps);
        flow = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(5, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync_Signal1:1", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("CallAsync_Update:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);

        var model = new SampleHandlingTimeoutFlow1();
        flow.ContextHistory.Last().Model.ExportTo(model);
        Assert.True(model.Timeout1);
        Assert.NotNull(model.TimeoutReachedOn);
        Assert.Equal(SampleHandlingTimeoutFlow1.Signal1, model.TimeoutReachedOnSignal);
    }

    public class SampleWaitingFlow1 : FlowBase
    {
        // signals
        public const string Signal1 = "Signal1";
        public const string Signal2 = "Signal2";

        public bool? Timeout1 { get; set; }

        public async Task Flow()
        {
            // pass first time out
            await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(0));
            Timeout1 = TimeoutOccurred;

            // stop here
            await WaitForSignalTimeoutAsync(Signal2, TimeSpan.FromSeconds(1));
            
            if (TimeoutOccurred == true)
            {
                await CallAsync(Cancel);
                return;
            }

            await CallAsync(Update);
        }

        private async Task Update()
        {
        }

        private async Task Cancel()
        {
        }
    }

    public class SampleHandlingTimeoutFlow1 : FlowBase
    {
        // signals
        public const string Signal1 = "Signal1";
        public DateTimeOffset? TimeoutReachedOn;
        public string? TimeoutReachedOnSignal;

        public bool? Timeout1 { get; set; }

        public async Task Flow()
        {
            AddSignalTimeoutHandler(SignalTimeoutHandler);

            // stop here
            await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(1));
            Timeout1 = TimeoutOccurred;

            await CallAsync(Update);
        }

        private async Task Update()
        {
        }

        private Task SignalTimeoutHandler(SignalPayload payload)
        {
            TimeoutReachedOn = payload.TimeoutReachedOn;
            TimeoutReachedOnSignal = payload.Signal;
            return Task.CompletedTask;
        }
    }

}
