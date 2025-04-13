using JsonPathToModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public class SignalPayload
{
    public const string JsonType = "json";

    public SnapshotRecord? Record { get; internal set; }

    public object? Value { get; internal set; }
    public string? Signal { get; internal set; }
    public DateTimeOffset? TimeoutReachedOn { get; internal set; }
    
    public T? GetValue<T>()
    {
        if (Record != null)
        {
            return Record.Deserialize<T>();
        }

        return (T?)Value;
    }
}
