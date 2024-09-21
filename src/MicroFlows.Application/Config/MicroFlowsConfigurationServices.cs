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
using System.Collections.Concurrent;
using System.Linq;
using JsonPathToModel;
using MicroFlows.Application.Exceptions;
using System.Data;
using System.Threading.Tasks;

namespace MicroFlows;

public static class MicroFlowsConfigurationServices
{
    internal static ConcurrentDictionary<Type, int> _registeredFlows = [];

    internal static HashSet<Type> GetRegisteredTypes()
    {
        return _registeredFlows.Keys.ToHashSet(); 
    }

    public static IServiceCollection RegisterFlow<T>(this IServiceCollection services) where T : class, IFlow
    {
        ValidateFlow(typeof(T));
        _registeredFlows.TryAdd(typeof(T), 0);
        services.AddTransient<T>();
        return services;
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services)
    {
        services.AddSingleton<IProxyGenerator, ProxyGenerator>();
        services.AddTransient<IFlowProvider, FlowProvider>();
        services.AddTransient<IFlowEngine, FlowEngine>();
        return services;
    }

    public static IServiceCollection AddMicroFlows(this IServiceCollection services, IConfiguration configuration)
    {
        AddMicroFlows(services);
        return services;
    }

    private static void ValidateFlow(Type type)
    {
        ShouldHaveCompatibleFlowMethod(type);
        ShouldNotHavePrivatePropertiesAndFields(type);
    }

    private readonly static ModelStateExplorer _expl = new();

    private static void ShouldNotHavePrivatePropertiesAndFields(Type type)
    {
        var ps = ModelSearchParams.FullModelState();
        ps.ExcludeStartsWith = "__";
        var stateResult = _expl.FindModelStateItems(type, ps);

        var properties = stateResult.Items.Where(i => i.Property != null);

        var privates = stateResult.Items.Where(i => i.Property != null
            && (i.Property.GetMethod.IsPrivate || i.Property.SetMethod.IsPrivate));

        if (privates.Any())
        {
            throw new FlowValidationException(
                $"{type.Name}: Private properties not supported, please modify '{privates.First().Property!.Name}'");
        }

        var privateFields = stateResult.Items.Where(i => i.Field != null && i.Field.IsPrivate);

        if (privateFields.Any())
        {
            throw new FlowValidationException(
                $"{type.Name}: Private fields not supported, please modify '{privateFields.First().Field!.Name}'");
        }
    }

    private static void ShouldHaveCompatibleFlowMethod(Type type)
    {
        var methods = type.GetMethods()
            .Where(m => m.Name == FlowEngine.FLOW_METHOD && m.GetParameters().Length == 0 && m.ReturnType == typeof(Task));

        var method = methods.FirstOrDefault();

        if (method == null && !IsFluentFlow(type))
        {
            throw new FlowValidationException(
                $"{type.Name}: Flow should have 'public Task Flow()' method or override 'public override void Define(IFlowBuilder builder)'");
        }
    }

    public static bool IsFluentFlow(Type type)
    {
        return type.GetMethod("Define")?.DeclaringType == type;
    }
}
