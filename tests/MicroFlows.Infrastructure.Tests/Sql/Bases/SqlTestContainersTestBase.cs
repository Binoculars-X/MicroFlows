using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using MicroFlows;
using MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;
using MicroFlows.Infrastructure.Tests.TestSampleFlows;
using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using Microsoft.Extensions.Logging.Abstractions;
using MicroFlows.Domain.Interfaces;

namespace MicroFlows.Infrastructure.Tests.Sql.Bases;

public class SqlTestContainersTestBase : IAsyncLifetime
{
    protected readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    protected IServiceProvider _services;

    public virtual void ConfigureSqlServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddMicroFlowsMsSqlRepo(configuration,
                    new MsSqlFlowRepositorySettings
                    {
                        ConnectionString = _msSqlContainer.GetConnectionString()
                    });
    }

    protected IFlowRepository _repo;

    protected IFlowEngine NewEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo,
            new IntegrationFlowTestEnvironment());
    }

    public async Task InitializeAsync()
    {
        IConfiguration? configuration = null;
        await _msSqlContainer.StartAsync();

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
                    .RegisterFlow<LinearInlineFlow>()
                    .RegisterFlow<LinearInlineFlow2>()
                    .RegisterFlow<SampleFlow>()
                    ;

                

                //services.TryAddScoped<IRepository, Repository>();

                //services.AddEntityFrameworkInMemoryDatabase();
                //services.AddDbContext<MyDbContext>((sp, options) =>
                //{
                //    options.UseSqlServer(_msSqlContainer.GetConnectionString());
                //});

                ConfigureSqlServices(configuration, services);
            })
            .Build();

        _services = host.Services;
        _repo = _services.GetService<IFlowRepository>()!;
        //_context = _services.GetService<MyDbContext>()!;
        //_repo = _services.GetService<IRepository>()!;
    }

    public Task DisposeAsync()
        => _msSqlContainer.DisposeAsync().AsTask();
}