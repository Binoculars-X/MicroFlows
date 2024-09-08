using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Domain.Enums;
using MicroFlows.Tests.Intercepting;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroFlows.Package.Tests;

public class SampleFlowTest : TestBase
{
    readonly MemoryFlowRepository _repo;

    public SampleFlowTest()
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
    public async Task SampleLoggingFlow_Not_Logging_WhenResumed()
    {
        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(SampleLoggingFlow), null);

        Assert.Equal(3, SampleLoggingFlow.Log.Count);

        // resume
        engine = GetEngine();
        SampleLoggingFlow.Log.Clear();
        var ps = new FlowParams() { RefId = ctx.RefId };
        await engine.ExecuteFlow(typeof(SampleLoggingFlow), ps);
        Assert.Empty(SampleLoggingFlow.Log);
    }
}