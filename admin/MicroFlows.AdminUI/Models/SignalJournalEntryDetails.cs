using MicroFlows.Domain.Models;

namespace MicroFlows.AdminUI.Models;

public class SignalJournalEntryDetails
{
    public SignalJournalEntry Entry;
    public bool Deleted;
    public bool Changed;

    public SignalJournalEntryDetails(SignalJournalEntry entry)
    {
        Entry = entry;
    }
}
