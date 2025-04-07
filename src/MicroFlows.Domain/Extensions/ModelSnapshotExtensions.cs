using JsonPathToModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MicroFlows;

public static class ModelSnapshotExtensions
{
    public static T? Deserialize<T>(this ModelSnapshot modelSnapshot) where T: class
    {
        return modelSnapshot.Records["$.Model"].Deserialize() as T;
    }

    /// <summary>
    /// Deserializes to a target type, supports restoring type from json 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="record"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T? Deserialize<T>(this SnapshotRecord record)
    {
        if (record.Type == SignalPayload.JsonType)
        {
            return JsonSerializer.Deserialize<T?>(record.Json);
        }

        return (T?)record.Deserialize();
    }

    

    public static string ToJson(this ModelSnapshot modelSnapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var record in modelSnapshot.Records)
        {
            var property = record.Key.Split('.').Last();
            var value = JsonPrettify(record.Value.Json);
            //var w = value.StartsWith("{") ? "" : "\"";
            sb.AppendLine($"  \"{property}\": {value},");
        }

        sb.AppendLine("}");
        var json = sb.ToString();//.Replace("},", "  },");
        var pretty = JsonPrettify(json);
        return pretty;
    }

    public static string JsonPrettify(string json)
    {
        using var jDoc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
        return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true, IndentSize = 2 });
    }
}
