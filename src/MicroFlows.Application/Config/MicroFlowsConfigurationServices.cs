using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Application.Engines;
using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application;

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
        ValidateFlow(typeof(T));
        _registeredFlows.Add(typeof(T));
        services.AddTransient<T>();
        return services;
    }

    private static void ValidateFlow(Type type)
    {
        // Add flow validation logic
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services)
    {
        services.AddSingleton<IProxyGenerator, ProxyGenerator>();
        services.AddSingleton<IFlowsProvider, FlowsProvider>();
        services.AddTransient<IFlowEngine, FlowEngine>();
        return services;
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services, IConfiguration configuration)
    {
        AddMicroFlows(services);
        return services;
    }
}
