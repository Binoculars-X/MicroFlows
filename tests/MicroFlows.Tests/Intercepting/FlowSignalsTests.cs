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
        Assert.Equal("WaitForSignalTimeoutAsync:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalTimeoutAsync:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        await Task.Delay(1100);
        engine = NewEngine();
        var ctx2 = await engine.ExecuteFlow(typeof(SampleSignalWaitingTimeoutFlow), ps);

        Assert.Equal(ctx.RefId, ctx2.RefId);

        flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal("WaitForSignalTimeoutAsync:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Anonymous:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
        
        var m1 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[1].Model.ExportTo(m1);
        Assert.Null(m1.Timeout1);
        Assert.Null(m1.Timeout2);

        var m2 = new SampleSignalWaitingTimeoutFlow();
        flow.ContextHistory[2].Model.ExportTo(m2);
        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory[2].ExecutionResult.FlowState);
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
}
