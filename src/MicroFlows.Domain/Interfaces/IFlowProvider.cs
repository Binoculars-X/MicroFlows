using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFlows;

public interface IFlowProvider
{
    //Task<FlowContext> CreateFlow(FlowParams flowParams);
    Task<FlowContext> CreateFlow(FlowParams flowParams, CancellationToken cancellationToken = default);
    //Task<FlowContext> ExecuteFlow(FlowParams flowParams);
    Task<FlowContext> ExecuteFlow(FlowParams flowParams, CancellationToken cancellationToken = default);
    Task<FlowContext> SendSignal(FlowParams flowParams, string signal, object? payload = null);
    Task<FlowContext> SendSignals(FlowParams flowParams, IDictionary<string, object?> signals);
}
