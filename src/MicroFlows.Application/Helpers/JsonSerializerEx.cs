using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace MicroFlows.Application.Helpers;

public static class JsonSerializerEx
{
    public static object? Deserialize(string json, string typeName)
    {
        var type = Type.GetType(typeName);

        // ToDo: find a proper method without using MetadataToken
        MethodInfo method = typeof(JsonSerializer)
            .GetMethods()
            .FirstOrDefault(x => x.Name == "Deserialize" && x.IsGenericMethod && x.MetadataToken == 100664404);

        MethodInfo generic = method.MakeGenericMethod(type);
        var obj = generic.Invoke(null, [json, null]);
        return obj;
    }
}
