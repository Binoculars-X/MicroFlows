using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;

public class ModelSnapshot
{
    public Dictionary<string, SnapshotRecord> Records { get; set; } = [];

    public void Clear()
    {
        Records.Clear();
    }
}

public record SnapshotRecord(string type, string Json);
