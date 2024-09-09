using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;
public interface IFlow
{
    //Task Execute();
    List<SignalJournalEntry> SignalJournal { get; }
    string RefId { get; set; }
}
