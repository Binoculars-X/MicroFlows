using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MicroFlows.Domain.Models;

public class SignalJournalEntry
{
    public string? Signal { get; set; }
    public DateTime? Received { get; set; } = DateTime.UtcNow;

    public SnapshotRecord? Record { get; set; }

    public SignalJournalEntry()
    { 
    }

    public SignalJournalEntry(string signal, object? payload)
    {
        Signal = signal;

        if (payload != null)
        {
            Record = new SnapshotRecord(payload.GetType().FullName, JsonSerializer.Serialize(payload));
        }
    }
}
