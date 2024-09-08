using MicroFlows.Domain.Enums;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowEngineTests
{
	[Fact]
	public async Task SampleSignalWaitingFlow_ShouldStop_and_RerunOnSignal()
	{
		var date = new DateTime(1974,10,15);
		var ctx = await _engine.ExecuteFlow(typeof(SampleSignalWaitingFlow), null);
		var flow = await _repo.GetFlowModel(ctx.RefId);

		Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);
		//Assert.Equal(3, flow.ContextHistory.Count);

		// resume and catch exception
		var ps = new FlowParams() { RefId = ctx.RefId };
		ctx = await _engine.SendSignal(typeof(SampleSignalWaitingFlow), SampleSignalWaitingFlow.Signal1, ps, date);
		Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
		Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
	}
}
