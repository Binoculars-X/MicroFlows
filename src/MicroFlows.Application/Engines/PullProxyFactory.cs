using BlazorForms.Proxyma

using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Engines;
public class PullProxyFactory : IProxymaProvider
{
    private readonly IProxymaProvider _proxymaProvider = new PullProxymaProvider(new ProxyGenerator(),
        MicroFlowsConfigurationServices.GetRegisteredTypes());

    public object CreateClassProxy(Type classToProxy, object[] constructorArguments, object target, ProxyGenerationOptions options, IAsyncInterceptor[] interceptors)
    {
        return _proxymaProvider.CreateClassProxy(classToProxy, constructorArguments, target, options, interceptors);
    }

    public IProxymaInterceptor CreateModelProxyInterceptor(Action<string, string, object> onModelChanging)
    {
        return _proxymaProvider.CreateModelProxyInterceptor(onModelChanging);
    }

    public T CreateModelProxyObject<T>(T source, IProxymaInterceptor interceptor) where T : class
    {
        return _proxymaProvider.CreateModelProxyObject(source, interceptor);
    }

    public T GetModelProxy<T>(T source, Action<string, string, object> changingProperty) where T : class
    {
        return _proxymaProvider.GetModelProxy(source, changingProperty);
    }

    public T GetProxyModel<T>(T source) where T : class
    {
        return _proxymaProvider.GetProxyModel(source);
    }
}
