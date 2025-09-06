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
using MicroFlows.Tests.Fluent;
using static MicroFlows.Tests.Intercepting.ModelSnapshotTests;
using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using MicroFlows.Tests.Intercepting;
using static MicroFlows.Tests.Intercepting.FlowSignalsTests;
using MicroFlows.Tests.UseCases.Examples;
using MicroFlows.UnitTesting;
using MicroFlows.Tests.Activities;
using static MicroFlows.Tests.Activities.FlowEngineActivitiesTests;

namespace MicroFlows.Tests;

public abstract class TestBase
{
    protected IServiceProvider _services = null!;

    //protected IFlowRepository _repo;
    internal MemoryFlowRepository _repo;

    protected IFlowEngine NewEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo,
            new IntegrationFlowTestEnvironment());
    }

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
                    .RegisterFlow<SampleSignalWaitingTimeoutFlow>()
                    .RegisterFlow<SampleWaitingFlow1>()
                    .RegisterFlow<SampleHandlingTimeoutFlow1>()

                    .RegisterFlow<SampleCheckSignalFlow>()
                    .RegisterFlow<SampleNonPublicFieldsFlow>()
                    .RegisterFlow<SampleWithDependenciesFlow>()
                    .RegisterFlow<SampleWithNonReadonlyDependenciesFlow>()
                    .RegisterFlow<SampleTypedModelFlow>()
                    
                    .RegisterFlow<SampleWithExceptionInBodyFlow>()
                    .RegisterFlow<SampleWithExceptionInDelegateFlow>()

                    .RegisterFlow<TypedModelFlow>()
                    .RegisterFlow<TypedModelInlineFlow>()
                    .RegisterFlow<UntypedModelInlineFlow>()

                    .RegisterFlow<SampleFluentFlow>()
                    .RegisterFlow<FluentFlowExecutionTests.LinearFlow>()
                    .RegisterFlow<FluentFlowExecutionTests.LinearInlineFlow>()
                    .RegisterFlow<FluentFlowExecutionTests.ConditionalInlineFlow>()
                    .RegisterFlow<FluentFlowExecutionTests.ConditionalParametrizedFlow>()
                    .RegisterFlow<RefundRequestFluentFlow>()

                    .RegisterFlow<SampleCheckSignalActivityFlow>()
                    .RegisterFlow<SimpleActivityFlow>()
                    ;

                services.AddSingleton<IFlowRepository, MemoryFlowRepository>();
                services.AddSingleton<IFlowTestEnvironment, IntegrationFlowTestEnvironment>();
            })
            .Build();

        _services = app.Services;
        _repo = _services.GetService<IFlowRepository>() as MemoryFlowRepository;
    }
}
