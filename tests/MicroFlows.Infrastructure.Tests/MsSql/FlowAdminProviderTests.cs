using MicroFlows.Domain.Enums;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class FlowAdminProviderTests : SqlTestContainersTestBase
{
    [Fact]
    public async Task Rerun_Failed_Flow_with_Failed_LastContext_Should_Fail_Again()
    {
        var engine = NewEngine();
        var ps = new FlowParams();
        ps["IsError"] = "true";
        WithExceptionStepFlow.InitCount = 0;
        WithExceptionStepFlow.CheckErrorCount = 0;
        WithExceptionStepFlow.Failures = 0;

        var ctx = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Failed, ctx.ExecutionResult.FlowState);
        Assert.Equal(1, WithExceptionStepFlow.CheckErrorCount);
        // Failed step is not added to the call stack
        Assert.Single(ctx.CallStack);

        var model = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(model);
        Assert.Equal(3, model.ContextHistory.Count);
        Assert.Equal("Begin:0", model.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", model.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_CheckError:2", model.ContextHistory[2].CurrentTask);

        // rerun flow
        ps.RefId = ctx.RefId;
        // changing parameters should not affect on flow replay
        ps["IsError"] = "false";
        var ctx2 = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.Equal(2, WithExceptionStepFlow.CheckErrorCount);
        Assert.Equal(2, WithExceptionStepFlow.Failures);
        Assert.Equal(1, WithExceptionStepFlow.InitCount);
        Assert.Equal(FlowStateEnum.Failed, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task Rerun_Failed_Flow_with_Failed_LastContext_Should_Pass_after_Model_Correction()
    {
        var engine = NewEngine();
        var ps = new FlowParams();
        ps["IsError"] = "true";
        WithExceptionStepFlow.CheckErrorCount = 0;
        WithExceptionStepFlow.Failures = 0;

        var ctx = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Failed, ctx.ExecutionResult.FlowState);
        Assert.Equal(1, WithExceptionStepFlow.CheckErrorCount);
        // Failed step is not added to the call stack
        Assert.Single(ctx.CallStack);

        var model = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(model);
        Assert.Equal(3, model.ContextHistory.Count);
        Assert.Equal("Begin:0", model.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", model.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_CheckError:2", model.ContextHistory[2].CurrentTask);

        // correct model
        var ms = model.ContextHistory[1].Model;
        ms.Records["$.IsError"] = ms.Records["$.IsError"] with { Json = "false" };
        await _admin.UpdateFlowContextModel(ctx.RefId, 1, ms);

        // rerun flow
        ps.RefId = ctx.RefId;
        var ctx2 = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.Equal(1, WithExceptionStepFlow.InitCount);
        Assert.Equal(2, WithExceptionStepFlow.CheckErrorCount);
        Assert.Equal(1, WithExceptionStepFlow.Failures);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
        Assert.Equal(3, ctx2.CallStack.Count);
        Assert.Equal("CallAsync_Finish:3", ctx2.CallStack[2]);
    }

    [Fact]
    public async Task Rerun_Failed_Flow_after_Deleting_Steps_Should_Fail_Again()
    {
        var engine = NewEngine();
        var ps = new FlowParams();
        ps["IsError"] = "true";
        WithExceptionStepFlow.InitCount = 0;
        WithExceptionStepFlow.CheckErrorCount = 0;
        WithExceptionStepFlow.Failures = 0;

        var ctx = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Failed, ctx.ExecutionResult.FlowState);
        Assert.Equal(1, WithExceptionStepFlow.CheckErrorCount);
        Assert.Equal(1, WithExceptionStepFlow.InitCount);
        // Failed step is not added to the call stack
        Assert.Single(ctx.CallStack);

        var model = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(model);
        Assert.Equal(3, model.ContextHistory.Count);
        Assert.Equal("Begin:0", model.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", model.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_CheckError:2", model.ContextHistory[2].CurrentTask);

        // delete execution steps
        await _admin.DeleteExecutionSteps(ctx.RefId, 1);
        
        // check deleted
        model = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(model);
        Assert.Equal(1, model.ContextHistory.Count);
        Assert.Equal("Begin:0", model.ContextHistory[0].CurrentTask);

        // rerun flow
        ps.RefId = ctx.RefId;
        // changing supplied parameters should not affect on flow replay
        ps["IsError"] = "false";
        var ctx2 = await engine.ExecuteFlow(typeof(WithExceptionStepFlow), ps);
        Assert.Equal(2, WithExceptionStepFlow.InitCount);
        Assert.Equal(2, WithExceptionStepFlow.CheckErrorCount);
        Assert.Equal(2, WithExceptionStepFlow.Failures);
        Assert.Equal(FlowStateEnum.Failed, ctx2.ExecutionResult.FlowState);
    }

    [Fact]
    public async Task Rerun_Flow_with_Failed_Status_and_LastContext_Continue_Should_()
    {
    }

    public class WithExceptionStepFlow : FlowBase
    {
        internal static int InitCount = 0;
        internal static int CheckErrorCount = 0;
        internal static int Failures = 0;

        protected bool IsError { get; set; }

        public async Task Flow()
        {
            await CallAsync(Init);
            await CallAsync(CheckError);
            await CallAsync(Finish);
        }

        private async Task Init()
        {
            InitCount++;
            IsError = Convert.ToBoolean(Params["IsError"]);
        }

        private async Task Finish()
        {
        }

        private async Task CheckError()
        {
            CheckErrorCount++;
            if (IsError)
            {
                Failures++;
                throw new Exception("IsError: true");
            }
        }
    }
}
