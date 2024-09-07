using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application;

public class FlowsProvider : IFlowsProvider
{
    private readonly Logger<FlowsProvider> _logger;
    private readonly IFlowEngine _interceptEngine;

    public FlowsProvider(Logger<FlowsProvider> logger, IFlowEngine interceptEngine)
    {
        _logger = logger;
        _interceptEngine = interceptEngine;
    }

    public async Task<FlowContext> ExecuteFlow(Type flowType, FlowParams? flowParams = null)
    {
        return await _interceptEngine.ExecuteFlow(flowType, flowParams);
    }

    public async Task<FlowContext> ExecuteFlow(string flowTypeName, FlowParams? flowParams = null)
    {
        return await _interceptEngine.ExecuteFlow(flowTypeName, flowParams);
    }
}
