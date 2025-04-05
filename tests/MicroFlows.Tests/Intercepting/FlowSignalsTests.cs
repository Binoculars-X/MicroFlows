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
        //Assert.Equal(4, flow.ContextHistory.Count);
        //Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        //Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);
        //Assert.Equal("WaitForSignalAsync:3", flow.ContextHistory[3].CurrentTask);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        await Task.Delay(1100);
        engine = NewEngine();
        var ctx2 = await engine.ExecuteFlow(typeof(SampleSignalWaitingTimeoutFlow), ps);

        Assert.Equal(ctx.RefId, ctx2.RefId);

        flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        //Assert.Equal("Call_Init:1", flow.ContextHistory[1].CurrentTask);
        //Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);
        //Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        //Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);
    }
}
