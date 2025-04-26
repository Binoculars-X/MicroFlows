using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MicroFlows;
using MicroFlows.Domain.Interfaces;
using System;
using MicroFlows.Demo.Flows;
using Microsoft.Extensions.DependencyInjection;
using MicroFlows.UnitTesting;

namespace MicroFlows.Demo.Tests.Helpers;

public class TestServices
{
    public static IServiceProvider CreateInMemory()
    {
        return Create((services, configuration) =>
        {
            services.AddSingleton<IFlowRepository, MemoryFlowRepository>();
            services.AddSingleton<IFlowTestEnvironment, IntegrationFlowTestEnvironment>();
        });
    }

    public static IServiceProvider CreateSql(string connectionStraing)
    {
        return Create((services, configuration) =>
        {
            services.AddMicroFlowsMsSqlRepo(configuration,
                    new MsSqlFlowRepositorySettings
                    {
                        ConnectionString = connectionStraing
                    });

            services.AddSingleton<IFlowTestEnvironment, IntegrationFlowTestEnvironment>();
        });
    }

    public static IServiceProvider Create(Action<IServiceCollection, IConfiguration>? action = null)
    {
        IConfiguration? configuration = null;

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                config.AddConfiguration(hostingContext.Configuration);
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile($"appsettings.Development.json", true, true);
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddMicroFlows(configuration)
                    .RegisterFlow<HotelBookingFlow>()
                    ;

                if (action != null)
                {
                    action(services, configuration);
                }
            })
            .Build();

        return host.Services;
    }
}
