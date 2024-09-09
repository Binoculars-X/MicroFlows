using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using JsonPathToModel;
using System.Text.Json;

namespace MicroFlows.Application.Helpers;
public static class ModelSnapshotExtensions
{
    private static JsonPathModelNavigator _navi = 
        new JsonPathModelNavigator(new NavigatorConfigOptions() { OptimizeWithCodeEmitter = true });

    public static void ExportTo(this ModelSnapshot model, IFlow flow)
    {
        try
        {
            foreach (var item in model.Records)
            {
                var mapping = item.Key;
                var record = item.Value;
                var value = JsonSerializerEx.Deserialize(record.Json, record.type);

                _navi.SetValue(flow, mapping, value);
            }
        }
        catch (Exception exc)
        {
            throw;
        }
    }

    public static void ImportFrom(this ModelSnapshot model, IFlow flow)
    {
        model.Clear();
        var mappings = ReflectionHelper.GetTypeNestedPropertyJsonPaths(flow.GetType());
        //mappings.Add("$.SignalJournal");

        foreach (var mapping in mappings)
        {
            var type = _navi.GetPropertyInfo(flow.GetType(), mapping)?.PropertyType;
            var value = _navi.GetValue(flow, mapping);
            var json = JsonSerializer.Serialize(value);
            model.Records[mapping] = new SnapshotRecord(type.FullName, json);
        }
    }

    public static object? Deserialize(this SnapshotRecord record)
    {
        var value = JsonSerializerEx.Deserialize(record.Json, record.type);
        return value;
    }

    //public static void ExportTo(this ModelSnapshot model, IFlow flow)
    //{
    //    try
    //    {
    //        foreach (var keyValue in model.Values)
    //        {
    //            _navi.SetValue(flow, keyValue.Key, keyValue.Value);
    //        }
    //    }
    //    catch (Exception exc)
    //    {
    //        throw;
    //    }
    //}

    //public static void ImportFrom(this ModelSnapshot model, IFlow flow)
    //{
    //    model.Values.Clear();
    //    var mappings = ReflectionHelper.GetTypeNestedPropertyJsonPaths(flow.GetType());

    //    foreach (var mapping in mappings)
    //    {
    //        model.Values[mapping] = _navi.GetValue(flow, mapping)?.ToString();
    //    }
    //}
}
