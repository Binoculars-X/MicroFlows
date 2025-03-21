using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MicroFlows.Domain.Interfaces;
using MicroFlows.MsSqlRepo;

namespace MicroFlows;

public static class ConfigurationServices
{
    public static IServiceCollection AddMicroFlowsMsSqlRepo(this IServiceCollection services, IConfiguration configuration,
        MsSqlFlowRepositorySettings? settings = null)
    {
        services.AddSingleton<IFlowRepository>(new MsSqlFlowRepository(configuration, settings));
        return services;
    }
}
