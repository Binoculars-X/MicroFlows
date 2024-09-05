using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
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

        // resume
        ps["flag"] = "don't stop";
        ps.RefId = ctx.RefId;
        var ctx2 = await engine.ExecuteFlow(typeof(SampleFlow), ps);
    }
}
