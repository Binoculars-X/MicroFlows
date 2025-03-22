using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using MicroFlows.Infrastructure.Tests.TestSampleFlows;
using MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class MsSqlFlowExecutionTests : SqlTestContainersTestBase
{
    private IFlowRepository _repo;

    private FlowEngine GetEngine()
    {
        _repo = _services.GetService<IFlowRepository>()!;

        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    [Fact]
    public async Task NoStorage_FluentFlow_Executed_Without_Storing_to_Db()
    {
        var engine = GetEngine();
        var ps = new FlowParams();
        ps.FlowOptions.NoStorage = true;

        var ctx = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        var model = await _repo.GetFlowModel(ctx.RefId);
        Assert.Null(model);
    }

    [Fact]
    public async Task FluentFlow_Execution_Stored_to_Db()
    {
        var engine = GetEngine();
        var ps = new FlowParams();

        var ctx = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        var model = ctx.Model.Deserialize<LinearModel>()!;
        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.True(model.NextExecuted);
        Assert.True(model.Next2Executed);

        var flowModel = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(flowModel);

        var lastCtx = flowModel.ContextHistory.Last();
        Assert.Equal(FlowStateEnum.Finished, lastCtx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, lastCtx.ExecutionResult.ResultState);
        
        model = lastCtx.Model.Deserialize<LinearModel>()!;
        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.True(model.NextExecuted);
        Assert.True(model.Next2Executed);
    }

    [Fact]
    public async Task SampleFlow_Execution_Stored_to_Db()
    {
        var engine = GetEngine();
        var ps = new FlowParams();

        var ctx = await engine.ExecuteFlow(typeof(SampleFlow), ps);
        Assert.NotNull(ctx);
        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        var model = new SampleFlow();
        ctx.Model.ExportTo(model);

        Assert.True(model.InitPassed);
        Assert.True(model.UpdatePassed);
        Assert.True(model.InitPassed);
        Assert.True(model.CallInlinePassed);

        var flowModel = await _repo.GetFlowModel(ctx.RefId);
        Assert.NotNull(flowModel);

        var lastCtx = flowModel.ContextHistory.Last();
        Assert.Equal(FlowStateEnum.Finished, lastCtx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, lastCtx.ExecutionResult.ResultState);
        
        lastCtx.Model.ExportTo(model);
        Assert.True(model.InitPassed);
        Assert.True(model.UpdatePassed);
        Assert.True(model.InitPassed);
        Assert.True(model.CallInlinePassed);
    }
}
