using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Interceptors;
public class InterceptorFlowRunEngineTests : TestBase
{
    [Fact]
    public async Task Test1()
    {
        //var repo = new Mock<IFlowRepository>();

        //repo.Setup(r => r.CreateFlowContext(It.IsAny<FlowBase>(), It.IsAny<FlowParams>()))
        //    .Returns(Task.FromResult(new FlowContext()
        //    {
        //        Model = 
        //    }));

        var repo = new MemoryFlowRepository();

        var engine = new InterceptorFlowRunEngine(new NullLogger<InterceptorFlowRunEngine>(),
            _services,
            new ProxyGenerator(),
            repo);

        var ps = new FlowParams();
        ps["flag"] = "stop";
        var ctx = await engine.ExecuteFlow(typeof(SampleFlow), ps);

        var flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(FlowStateEnum.Stop, flow.ContextHistory.Last().ExecutionResult.FlowState);
        Assert.Equal("FlowStopException", flow.ContextHistory.Last().ExecutionResult.ExceptionType);
        Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory.Last().CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[1].ExecutionResult.FlowState);
        Assert.Equal("CallAsync<Flow>b__17_0:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal(FlowStateEnum.Continue, flow.ContextHistory[2].ExecutionResult.FlowState);
        Assert.Equal("CallAsync<Flow>b__17_1:2", flow.ContextHistory[2].CurrentTask);

        // resume
        ps["flag"] = "don't stop";
        ps.RefId = ctx.RefId;
        var ctx2 = await engine.ExecuteFlow(typeof(SampleFlow), ps);
    }
}
