using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public interface IFlowProvider
{
    Task<FlowContext> ExecuteFlow(FlowParams flowParams);
    Task<FlowContext> SendSignal(FlowParams flowParams, string signal, object? payload = null);
    Task<FlowContext> SendSignals(FlowParams flowParams, IDictionary<string, object?> signals);
}
