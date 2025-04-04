using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using JsonPathToModel;

namespace MicroFlows.Domain.Models;

public class SignalJournalEntry
{
    public string? Signal { get; set; }
    public DateTimeOffset? Received { get; set; }
    public DateTimeOffset? TimeoutReachedOn { get; set; }

    public SnapshotRecord? Record { get; set; }

    public SignalJournalEntry()
    { 
    }

    public SignalJournalEntry(string signal, object? payload)
    {
        Signal = signal;
        Received = DateTime.UtcNow;

        if (payload != null)
        {
            Record = new SnapshotRecord(payload.GetType().FullName, JsonSerializer.Serialize(payload));
        }
    }
}
