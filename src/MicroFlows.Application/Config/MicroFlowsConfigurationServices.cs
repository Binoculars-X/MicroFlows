using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using MicroFlows.Domain.Interfaces;
using BlazorForms.Proxyma;
using MicroFlows.Application.Engines;

namespace MicroFlows;

public static class MicroFlowsConfigurationServices
{
    internal static HashSet<Type> _registeredFlows = [];

    internal static HashSet<Type> GetRegisteredTypes()
    {
        return _registeredFlows; 
    }

    public static IServiceCollection RegisterFlow<T>(this IServiceCollection services) where T : class, IFlow
    {
        _registeredFlows.Add(typeof(T));
        return services;
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services)
    {
        services.AddSingleton<IProxymaProvider, PullProxyFactory>();
        services.AddTransient<IFlowRunEngine, InterceptorFlowRunEngine>();
        return services;
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services, IConfiguration configuration)
    {
        AddMicroFlows(services);
        return services;
    }
}
