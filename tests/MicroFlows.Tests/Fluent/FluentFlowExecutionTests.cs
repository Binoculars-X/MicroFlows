using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Domain.Enums;
using MicroFlows.Tests.Intercepting;
using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.Tests.TestSampleFlows.Fluent;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Fluent;

public class FluentFlowExecutionTests : TestBase
{
    readonly MemoryFlowRepository _repo;

    private FlowEngine GetEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    public FluentFlowExecutionTests()
    {
        _repo = new MemoryFlowRepository();
    }

    [Fact]
    public async Task LinearFlow_Executed_Setting_Model()
    {
        var engine = GetEngine();

        var ps = new FlowParams
        {
            FlowType = typeof(LinearFlow),
        };

        var ctx = await engine.ExecuteFluentFlow(ps);

        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        // example to get model
        var model = ctx.Model.Records["$.Model"].Deserialize() as LinearModel;

        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.True(model.NextExecuted);
        Assert.True(model.Next2Executed);

        // another example to get model
        var result = new LinearFlow();
        ctx.Model.ExportTo(result);
        var m = result.Model;

        Assert.True(m.FlowStartExecuted);
        Assert.True(m.FlowEndExecuted);
        Assert.True(m.NextExecuted);
        Assert.True(m.Next2Executed);
    }

    [Fact]
    public async Task LinearInlineFlow_Executed_Setting_Model()
    {
        var engine = GetEngine();

        var ps = new FlowParams
        {
            FlowType = typeof(LinearInlineFlow),
        };

        var ctx = await engine.ExecuteFluentFlow(ps);

        // another example to get model via wrapper
        var result = new LinearModelWrapper();
        ctx.Model.ExportTo(result);
        var m = result.Model;

        Assert.True(m.FlowStartExecuted);
        Assert.True(m.FlowEndExecuted);
        Assert.True(m.NextExecuted);
        Assert.True(m.Next2Executed);
    }

    [Fact]
    public async Task ConditionalFlow_Executed_If_Condition()
    {
        var engine = GetEngine();

        var ps = new FlowParams
        {
            FlowType = typeof(ConditionalInlineFlow),
        };

        var ctx = await engine.ExecuteFluentFlow(ps);

        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        // example to get model
        var model = ctx.Model.Records["$.Model"].Deserialize() as LinearModel;

        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.True(model.NextExecuted);
        Assert.False(model.Next2Executed);
    }

    [Fact]
    public async Task ConditionFlow_with_Params_Executed_If_Condition()
    {
        var engine = GetEngine();

        var ps = new FlowParams
        {
            FlowType = typeof(ConditionalParametrizedFlow),
        };

        ps["Condition1"] = "true";
        var ctx = await engine.ExecuteFluentFlow(ps);

        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        // example to get model
        var model = ctx.Model.Deserialize<LinearModel>();

        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.True(model.NextExecuted);
        Assert.False(model.Next2Executed);

        ps["Condition1"] = "false";
        ctx = await engine.ExecuteFluentFlow(ps);

        Assert.Equal(FlowStateEnum.Finished, ctx.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);

        // example to get model
        model = ctx.Model.Deserialize<LinearModel>();

        Assert.True(model.FlowStartExecuted);
        Assert.True(model.FlowEndExecuted);
        Assert.False(model.NextExecuted);
        Assert.True(model.Next2Executed);
    }

    public class LinearModel
    {
        public bool FlowStartExecuted { get; set; }
        public bool NextExecuted { get; set; }
        public bool Next2Executed { get; set; }
        public bool FlowEndExecuted { get; set; }
        public bool Condition1 { get; set; }
    }

    public class LinearModelWrapper
    {
        public LinearModel Model { get; set; }
    }

    public class LinearFlow : FlowBase<LinearModel>
    {
        public override void Define(IFlowBuilder builder)
        {
            builder
                .Begin(FlowStart)
                .Next(() => { Model.NextExecuted = true; })
                .Next(() => { Model.Next2Executed = true; })
                .End(FlowEnd);
        }

        public async Task FlowStart()
        {
            Model.FlowStartExecuted = true;
        }

        public async Task FlowEnd()
        {
            Model.FlowEndExecuted = true;
        }
    }

    public class LinearInlineFlow : FlowBase<LinearModel>
    {
        public override void Define(IFlowBuilder builder)
        {
            builder
                .Begin(() => { Model.FlowStartExecuted = true; })
                .Next(() => { Model.NextExecuted = true; })
                .Next(() => { Model.Next2Executed = true; })
                .End(() => { Model.FlowEndExecuted = true; });
        }
    }

    public class ConditionalInlineFlow : FlowBase<LinearModel>
    {
        public override void Define(IFlowBuilder builder)
        {
            builder
                .Begin(() => { Model.FlowStartExecuted = true; })
                .If(() => true)
                    .Next(() => { Model.NextExecuted = true; })
                .Else()
                    .Next(() => { Model.Next2Executed = true; })
                .EndIf()
                .End(() => { Model.FlowEndExecuted = true; });
        }
    }

    public class ConditionalParametrizedFlow : FlowBase<LinearModel>
    {
        public override void Define(IFlowBuilder builder)
        {
            builder
                .Begin(() => 
                { 
                    Model.FlowStartExecuted = true;
                    Model.Condition1 = Params["Condition1"] == "true";
                })
                .If(() => Model.Condition1)
                    .Next(() => { Model.NextExecuted = true; })
                .Else()
                    .Next(() => { Model.Next2Executed = true; })
                .EndIf()
                .End(() => { Model.FlowEndExecuted = true; });
        }
    }
}
