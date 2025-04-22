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
    Task UpdateFlowContextModel(string RefId, int index, ModelSnapshot updated);
    Task UpdateFlowContextParams(string RefId, int index, FlowParams updated);
    //Task UpdateFlowStatus(string RefId, FlowStateEnum status);
    Task HaltFlow(string RefId);
    Task RerunFlow(string RefId);
    Task UpdateFlowSignals(string RefId, List<SignalJournalEntry> journal);
}
