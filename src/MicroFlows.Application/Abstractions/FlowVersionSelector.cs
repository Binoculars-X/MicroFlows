using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public abstract class FlowVersionSelector : IFlow
{
    public List<SignalJournalEntry> SignalJournal => throw new NotImplementedException();

    public string RefId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public abstract void RegisterFlowVersion(List<Type> versions);
}
