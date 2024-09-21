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
    public async Task FlowEngine_Should_Support_NonPublics_InModel()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleNonPublicFieldsFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);

        var last = flow.ContextHistory.Last();
        Assert.Equal(4, last.Model.Records.Count());
    }

    [Fact]
    public async Task FlowEngine_Should_Support_FlowsWithDependencies()
    {
        //var services = ConfigHelper.GetConfigurationServices();
        //var logger = services.GetService<ILogger<FlowsProvider>>();
        //var prvd = services.GetService<IFlowsProvider>();

        //var engine = new FlowEngine(new NullLogger<FlowEngine>(),
        //    _services,
        //    new ProxyGenerator(),
        //    _repo);
        var engine = GetEngine();

        var ctx = await engine.ExecuteFlow(typeof(SampleWithDependenciesFlow), null);
        var flow = await _repo.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_Update:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:3", flow.ContextHistory[3].CurrentTask);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        var last = flow.ContextHistory.Last();
        Assert.Equal(4, last.Model.Records.Count());
    }

    [Fact]
    public async Task FlowEngine_Should_NotSupport_FlowsWith_NonReadonly_Dependencies()
    {
        var engine = GetEngine();

        var exc = await Assert.ThrowsAsync<TargetInvocationException>(async() => 
            await engine.ExecuteFlow(typeof(SampleWithNonReadonlyDependenciesFlow)));

        var inner = exc.InnerException;
        Assert.Contains("IFlowProvider", inner.Message);
        Assert.Contains("Deserialization of interface types is not supported", inner.Message);
    }
}
