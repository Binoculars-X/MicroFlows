using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;
public class FlowParams : Interfaces.IFlowParams
{
    public Dictionary<string, string> DynamicInput { get; private set; } = [];

    public FlowParams()
    {
    }

    public string ItemId { get; set; }

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

    public string ParentItemId { get; set; }
    public string AssignedUser { get; set; }
    public string AssignedTeam { get; set; }
    //public FlowReferenceOperation Operation { get; set; }
    public string Tag { get; set; }

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

    public string GetParam(string key)
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
