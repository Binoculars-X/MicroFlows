using JsonPathToModel;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public interface IFlowAdminProvider
{
    Task DeleteExecutionSteps(string RefId, int index);
    Task UpdateFlowContextModel(string RefId, int index, ModelSnapshot model);
    Task UpdateFlowStatus(string RefId, FlowStateEnum status);
    Task UpdateFlowSignals(string RefId, List<SignalJournalEntry> journal);
}
