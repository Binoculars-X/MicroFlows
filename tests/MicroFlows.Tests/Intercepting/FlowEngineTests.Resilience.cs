using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.Helpers;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MicroFlows.Tests.Intercepting;

public partial class FlowEngineTests
{
    [Fact]
    public async Task FlowEngine_Should_Be_Rerunnable_AfterExceptionInFlowBody()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleWithExceptionInBodyFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_Update:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(ResultStateEnum.Fail, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        var last = flow.ContextHistory.Last();
        Assert.Null(last.CurrentTask);
        Assert.NotNull(last.ExecutionResult.ExceptionMessage);
        Assert.Equal("Exception", last.ExecutionResult.ExceptionType);
        Assert.Equal(2, last.Model.Records.Count());
        Assert.Equal("id:test", last.Model.Records["$.Id"].Deserialize());
        Assert.Equal(33, last.Model.Records["$.ModelInt"].Deserialize());

        engine = GetEngine();
        var ctx2 = await engine.ExecuteFlow(typeof(SampleWithExceptionInBodyFlow), new FlowParams { RefId = ctx.RefId });
        flow = await _repo.GetFlowModel(ctx2.RefId);
        last = flow.ContextHistory.Last();

        Assert.Equal(5, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx2.ExecutionResult.FlowState);
        Assert.Equal("WaitForCondition:3", last.CurrentTask);
        Assert.Equal("WaitForCondition", last.ExecutionResult.ExceptionMessage);
        Assert.Equal("FlowStopException", last.ExecutionResult.ExceptionType);
        Assert.Equal(2, last.Model.Records.Count());
        Assert.Equal("recovered", last.Model.Records["$.Id"].Deserialize());
        Assert.Equal(33, last.Model.Records["$.ModelInt"].Deserialize());
    }

    [Fact]
    public async Task FlowEngine_Should_Be_Rerunnable_AfterExceptionInDelegate()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleWithExceptionInDelegateFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_Update:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal(ResultStateEnum.Fail, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        var last = flow.ContextHistory.Last();
        Assert.Equal("CallAsync_CheckStatus:3", last.CurrentTask);
        Assert.NotNull(last.ExecutionResult.ExceptionMessage);
        Assert.Equal("Some error code here", last.ExecutionResult.ExceptionMessage);
        Assert.Equal("Exception", last.ExecutionResult.ExceptionType);
        Assert.Equal(2, last.Model.Records.Count());
        Assert.Equal("id:test", last.Model.Records["$.Id"].Deserialize());
        Assert.Equal(33, last.Model.Records["$.ModelInt"].Deserialize());

        engine = GetEngine();
        var ctx2 = await engine.ExecuteFlow(typeof(SampleWithExceptionInDelegateFlow), new FlowParams { RefId = ctx.RefId });
        flow = await _repo.GetFlowModel(ctx2.RefId);

        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);
        Assert.Equal(6, flow.ContextHistory.Count);
        
        var prelast = flow.ContextHistory[4];
        Assert.Equal("CallAsync_CheckStatus:3", prelast.CurrentTask);
        Assert.Null(prelast.ExecutionResult.ExceptionMessage);
        Assert.Null(prelast.ExecutionResult.ExceptionType);
        Assert.Equal(2, prelast.Model.Records.Count());
        Assert.Equal("id:test", prelast.Model.Records["$.Id"].Deserialize());
        Assert.Equal(33, prelast.Model.Records["$.ModelInt"].Deserialize());

        // after flow finished we should be able to see the latest model changes in Flow body
        last = flow.ContextHistory.Last();
        Assert.Equal("recovered", last.Model.Records["$.Id"].Deserialize());
        Assert.Equal(414, last.Model.Records["$.ModelInt"].Deserialize());

        var callstack = flow.ContextHistory.Select(c => c.CurrentTask).Distinct().ToList();
        Assert.Equal(5, callstack.Count);
        Assert.Equal("End:4", callstack[4]);
    }
}
