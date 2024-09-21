using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
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

public class FluentFlowTests : TestBase
{
    readonly MemoryFlowRepository _repo;

    public FluentFlowTests()
    {
        _repo = new MemoryFlowRepository();
    }

    private FlowEngine GetEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    [Fact]
    public void FluentFlow_Should_Be_Detected_WhenDefineOverriden()
    {
        Assert.Equal(typeof(SampleFluentFlow), typeof(SampleFluentFlow).GetMethod("Define").DeclaringType);
    }

    [Fact]
    public void FluentFlow_ShouldNot_Be_Detected_WhenDefineNotOverriden()
    {
        Assert.Equal(typeof(FlowBase), typeof(SampleFlow).GetMethod("Define").DeclaringType);
    }

    [Fact]
    public async Task SimpleClientKeptContextTest()
    {
        var engine = GetEngine();

        var ps = new FlowParams
        {
            FlowType = typeof(SampleFluentFlow),
        };

        var data = await engine.GetFlowDefinitionDetails(ps);
        Assert.Equal(10, data.States.Count);
        Assert.Equal(10, data.Transitions.Count);
    }
}
