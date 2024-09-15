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

public partial class FlowEngineTests : TestBase
{
    [Fact]
    public async Task FlowEngine_Should_Support_TypedModelFlows()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleTypedModelFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_Update:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:3", flow.ContextHistory[3].CurrentTask);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        var last = flow.ContextHistory.Last();
        Assert.Single(last.Model.Records);
        
        var model = last.Model.Records["$.Model"].Deserialize() as SampleTypedModel;
        Assert.Equal("test", model!.Name);
        Assert.Null(model!.Id);
    }
}
