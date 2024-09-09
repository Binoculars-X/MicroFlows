using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MicroFlows.Domain.Models;

public class ModelSnapshot
{
    public Dictionary<string, SnapshotRecord> Records { get; set; } = [];

    public void Clear()
    {
        Records.Clear();
    }
}

public record SnapshotRecord(string? Type, string? Json)
{
    public SnapshotRecord() : this(null, null)
    {
    }

    public SnapshotRecord(object model) : 
        this(model.GetType().FullName, JsonSerializer.Serialize(model))
    { 
    }
}
