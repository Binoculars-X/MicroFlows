using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Application.Helpers;
public static class TypeHelper
{
    public static Type ResolveType(string name)
    {
        var type = Assembly.GetExecutingAssembly().GetType(name);
        return type;
    }

    public static object[] GetConstructorParameters(IServiceProvider serviceProvider, Type classType)
    {
        var methodParameters = classType.GetConstructors().FirstOrDefault()?.GetParameters();

        var parameters = new List<object>();

        foreach (var parameter in methodParameters)
        {
            var svc = serviceProvider.GetService(parameter.ParameterType);

            if (svc == null)
            {
                throw new InvalidDependencyException($"Cannot resolve dependency for type {parameter.ParameterType}");
            }

            parameters.Add(svc);
        }

        return parameters.ToArray();
    }

    public static bool IsAsyncMethod(MethodInfo method)
    {
        if (method.ReturnType?.Name != null && method.ReturnType.Name.StartsWith("Task"))
        {
            return true;
        }

        return false;
    }

    public static T CloneObject<T>(T source)
    {
        var json = JsonSerializer.Serialize(source);
        var result = JsonSerializer.Deserialize<T>(json);
        return result;
    }
}
