using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MicroFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.Tests.TestFlows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicroFlows.Tests.TestSampleFlows.Fluent;

namespace MicroFlows.Tests;
public abstract class TestBase
{
    protected IServiceProvider _services = null!;

    public TestBase()
    {
        IConfiguration configuration = null!;

        var app = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                config.AddConfiguration(hostingContext.Configuration);
                config.AddJsonFile("appsettings.json", true);
                config.AddJsonFile($"appsettings.Development.json", true, true);
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                //services.AddLogging(logging => logging.AddConsole());
                services.AddLogging();

                services.AddMicroFlows(configuration)
                    .RegisterFlow<SampleFlow>()
                    .RegisterFlow<SampleStoringFlow>()
                    .RegisterFlow<SampleWaitingFlow>()
                    .RegisterFlow<SampleLoggingFlow>()
                    .RegisterFlow<SampleExceptionFlow>()
                    .RegisterFlow<SampleExceptionInActionFlow>()
                    .RegisterFlow<SampleSignalWaitingFlow>()
                    .RegisterFlow<SampleTwoSignalsWaitingFlow>()
                    .RegisterFlow<SampleSignalPayloadWaitingFlow>()
                    .RegisterFlow<SampleTwoSignalPayloadWaitingFlow>()
                    .RegisterFlow<SampleCheckSignalFlow>()
                    .RegisterFlow<SampleNonPublicFieldsFlow>()
                    .RegisterFlow<SampleWithDependenciesFlow>()
                    .RegisterFlow<SampleWithNonReadonlyDependenciesFlow>()
                    .RegisterFlow<SampleTypedModelFlow>()
                    .RegisterFlow<SampleWithExceptionInBodyFlow>()
                    .RegisterFlow<SampleWithExceptionInDelegateFlow>()
                    .RegisterFlow<SampleFluentFlow>()
                    ;
                
            })
            .Build();

        _services = app.Services;
    }
}
