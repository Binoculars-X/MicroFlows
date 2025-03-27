using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MicroFlows.Domain.Interfaces;

namespace MicroFlows;
public class FlowParams : IFlowParams
{
    public Dictionary<string, string> DynamicInput { get; set; } = [];

    public string RefId { get; set; } = null!;
    public string ExternalId { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;
    public string FlowName { get; set; } = null!;
    
    /// <summary>
    /// Used internaly but should not be serialized
    /// </summary>
    //[JsonIgnore]
    internal Type? FlowType { get; set; } = null!;
    
    public FlowOptions FlowOptions { get; private set; } = new();

    // ToDo: do we need all them Item id and key?
    public string ItemId { get; set; } = null!;

    public bool ItemKeyAboveZero
    {
        get
        {
            int id = 0;
            int.TryParse(ItemId, out id);
            return id > 0;
        }
    }

    public int ItemKey
    {
        get
        {
            int id = 0;
            int.TryParse(ItemId, out id);
            return id;
        }
    }

    public string ParentItemId { get; set; } = null!;
    public string AssignedUser { get; set; } = null!;
    public string AssignedTeam { get; set; } = null!;
    //public FlowReferenceOperation Operation { get; set; }
    public string Tag { get; set; } = null!;

    public string this[string index]
    {
        get
        {
            return DynamicInput.TryGetValue(index, out var value) ? value : string.Empty;
        }

        set
        {
            DynamicInput[index] = value;
        }
    }

    public string? GetParam(string key)
    {
        if (DynamicInput.ContainsKey(key))
        {
            return DynamicInput[key];
        }

        return null;
    }

    public int? GetParamInt(string key)
    {
        var stringParam = GetParam(key);

        if (int.TryParse(stringParam, out int intParam))
        {
            return intParam;
        }

        return null;
    }
}
