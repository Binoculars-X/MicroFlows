using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.UnitTesting;
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
            _repo,
            null);
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

    [Fact]
    public void TimeZoneHelper_ConvertDateTimeToUtc_Returns_Utc()
    {
        var timeZone = "Australia/Sydney";
        var dateTime = DateTime.Parse("2025-04-19 16:34:04");
        var local = TimeZoneHelper.ConvertDateTimeToDateTimeOffset(dateTime, timeZone);
        var utcHours = 16 - local.Value.Offset.Hours;
        var utc = DateTimeOffset.Parse($"2025-04-19T{utcHours:D2}:34:04.0000000+00:00");
        var converted = TimeZoneHelper.ConvertDateTimeToUtc(dateTime, timeZone);

        Assert.Equal(utc.Date, converted.Value.Date);
        Assert.Equal(utc.Day, converted.Value.Day);
        Assert.Equal(utc.Hour, converted.Value.Hour);
        Assert.Equal(utc.Minute, converted.Value.Minute);
    }
}