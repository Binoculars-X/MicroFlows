using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using JsonPathToModel;

namespace MicroFlows.Application.Helpers;
public static class ModelSnapshotExtensions
{
    private static JsonPathModelNavigator _navi = 
        new JsonPathModelNavigator(new NavigatorConfigOptions() { OptimizeWithCodeEmitter = true });

    public static void ExportTo(this ModelSnapshot model, IFlow flow)
    {
        try
        {
            foreach (var keyValue in model.Values)
            {
                _navi.SetValue(flow, keyValue.Key, keyValue.Value);
            }
        }
        catch (Exception exc)
        {
            throw;
        }
    }

    public static void ImportFrom(this ModelSnapshot model, IFlow flow)
    {
        model.Values.Clear();
        var mappings = ReflectionHelper.GetTypeNestedPropertyJsonPaths(flow.GetType());

        foreach (var mapping in mappings)
        {
            model.Values[mapping] = _navi.GetValue(flow, mapping)?.ToString();
        }
    }
}
