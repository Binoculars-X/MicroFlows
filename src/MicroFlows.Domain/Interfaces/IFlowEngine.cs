using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;
public interface IFlowEngine
{
    //public Dictionary<string, object?> Signals { get; }

    Task<FlowContext> ExecuteFlow(Type flowType, FlowParams? flowParams = null);
    Task<FlowContext> SendSignal(Type flowType, string signal, FlowParams? flowParams = null, object? payload = null);
    Task<FlowContext> SendSignals(Type flowType, IDictionary<string, object?> signals, FlowParams? flowParams = null);
    //Task<FlowContext> ExecuteFlow(string flowTypeName, FlowParams? flowParams = null);
    //Task<FlowContext> ResumeFlow(string flowId, FlowParams? flowParams = null);
    Task<FlowDefinitionDetails> GetFlowDefinitionDetails(FlowParams runParameters);
}
