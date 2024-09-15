using MicroFlows.Domain.Enums;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowEngineTests
{
    [Fact]
    public async Task SampleSignalWaitingFlow_CanBeCreated_AndResumed_ByExternalId()
	{
        var engine = GetEngine();
        var ps = new FlowParams() { ExternalId = "ORDER-123" };
        var ctx = await engine.ExecuteFlow(typeof(SampleSignalWaitingFlow), ps);

        var flow = await _repo.GetFlowModel(ctx.RefId);
        Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal("Call_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        engine = GetEngine();
        ps = new FlowParams() { ExternalId = "ORDER-123" };
        var ctx2 = await engine.SendSignal(typeof(SampleSignalWaitingFlow), SampleSignalWaitingFlow.Signal1, ps);

        Assert.Equal(ctx.RefId, ctx2.RefId);
        flow = await _repo.GetFlowModel(ctx2.RefId);
        Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
	public async Task SampleSignalWaitingFlow_ShouldStop_and_RerunOnSignal()
	{
		var engine = GetEngine();
		var date = new DateTime(1974,10,15);
		var ctx = await engine.ExecuteFlow(typeof(SampleSignalWaitingFlow), null);
		var flow = await _repo.GetFlowModel(ctx.RefId);

		Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);
		Assert.Equal(3, flow.ContextHistory.Count);
        Assert.Equal("Call_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);

        // resume and catch exception
        engine = GetEngine();
        var ps = new FlowParams() { RefId = ctx.RefId };
		ctx = await engine.SendSignal(typeof(SampleSignalWaitingFlow), SampleSignalWaitingFlow.Signal1, ps, date);
		flow = await _repo.GetFlowModel(ctx.RefId);

		Assert.Equal(6, flow.ContextHistory.Count);
        Assert.Equal("Call_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Anonymous:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal("CallAsync_Anonymous:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal("End:4", flow.ContextHistory[5].CurrentTask);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
	}

	[Fact]
	public async Task SampleTwoSignalsWaitingFlow_ShouldStop_and_RerunOnSignals()
	{
		var engine = GetEngine();
		var date = new DateTime(1974, 10, 15);
		var ctx = await engine.ExecuteFlow(typeof(SampleTwoSignalsWaitingFlow), null);
		var flow = await _repo.GetFlowModel(ctx.RefId);

		Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);
		Assert.Equal(3, flow.ContextHistory.Count);

        // resume and catch exception
        engine = GetEngine();
        var ps = new FlowParams() { RefId = ctx.RefId };
		
		var signals = new Dictionary<string, object?> { { SampleTwoSignalsWaitingFlow.Signal1, date },
			{ SampleTwoSignalsWaitingFlow.Signal2, null }};
		
		ctx = await engine.SendSignals(typeof(SampleTwoSignalsWaitingFlow), signals, ps);
		flow = await _repo.GetFlowModel(ctx.RefId);

		Assert.Equal(7, flow.ContextHistory.Count);
		Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
		//Assert.Equal(date, DateTime.Parse(ctx.Model.Values["$.ModelDate"]));
        Assert.Equal("Call_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:2", flow.ContextHistory[3].CurrentTask);
        Assert.Equal("CallAsync_Anonymous:3", flow.ContextHistory[4].CurrentTask);
        Assert.Equal("WaitForSignalAsync:4", flow.ContextHistory[5].CurrentTask);
        Assert.Equal("End:5", flow.ContextHistory[6].CurrentTask);
    }
}
