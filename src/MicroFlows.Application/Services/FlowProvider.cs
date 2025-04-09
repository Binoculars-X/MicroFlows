using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MicroFlows.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Helpers;
using MicroFlows.Application.Exceptions;
using System.Threading;
using System.Linq;

namespace MicroFlows.Application.Services;

public class FlowProvider : IFlowProvider
{
    //private readonly Logger<FlowsProvider> _logger;
    private readonly IServiceProvider _services;
    private readonly IFlowRepository _flowRepository;

    public FlowProvider(
        //Logger<FlowsProvider> logger, 
        IServiceProvider services)
    {
        //_logger = logger;
        _services = services;
        _flowRepository = _services.GetService<IFlowRepository>()!;
    }

    public async Task<FlowContext> SendSignal(FlowParams flowParams, string signal, object? payload = null)
    {
        var interceptEngine = PrepareFlowEngine(flowParams);
        await interceptEngine.EnsureFlowExists(flowParams);
        return await interceptEngine.SendSignal(flowParams.FlowType!, signal, flowParams, payload);
    }

    public async Task<FlowContext> SendSignals(FlowParams flowParams, IDictionary<string, object?> signals)
    {
        var interceptEngine = PrepareFlowEngine(flowParams);
        await interceptEngine.EnsureFlowExists(flowParams);
        return await interceptEngine.SendSignals(flowParams.FlowType!, signals, flowParams);
    }

    /// <summary>
    /// Executes an existing or creates a new flow and executes
    /// </summary>
    /// <param name="flowParams"></param>
    /// <returns></returns>
    public async Task<FlowContext> ExecuteFlow(FlowParams flowParams, CancellationToken cancellationToken = default)
    {
        var interceptEngine = PrepareFlowEngine(flowParams);
        return await interceptEngine.ExecuteFlow(flowParams.FlowType!, flowParams);
    }

    /// <summary>
    /// Creates flow, saves it to repo but doesn't run
    /// </summary>
    /// <param name="flowParams"></param>
    /// <returns></returns>
    public async Task<FlowContext> CreateFlow(FlowParams flowParams, CancellationToken cancellationToken = default)
    {
        var interceptEngine = PrepareFlowEngine(flowParams);
        return await interceptEngine.CreateFlow(flowParams.FlowType!, flowParams);
    }

    private IFlowEngine PrepareFlowEngine(FlowParams flowParams)
    {
        var interceptEngine = _services.GetService<IFlowEngine>();

        if (interceptEngine == null)
        {
            throw new InvalidDependencyException("IFlowEngine is not registered");
        }

        if (flowParams.FlowType == null)
        {
            var type = TypeHelper.ResolveType(flowParams.FlowName);

            if (type == null)
            {
                type = MicroFlowsConfigurationServices.GetFlowType(flowParams.FlowName);
            }

            if (type == null)
            {
                throw new InvalidDependencyException($"Flow '{flowParams.FlowName}' is not registered");
            }

            flowParams.FlowType = type;
        }

        return interceptEngine;
    }

    public async Task SendSignalJson(string refId, string signal, string? payloadJson = null)
    {
        var model = await _flowRepository.GetFlowModel(refId);
        var journal = model.SignalJournal.FirstOrDefault(j => j.Signal == signal);

        if (journal == null)
        {
            journal = new SignalJournalEntry { Signal = signal, Received = DateTimeOffset.UtcNow };
            model.SignalJournal.Add(journal);
        }

        journal.Record = new JsonPathToModel.SnapshotRecord(SignalPayload.JsonType, payloadJson);
        await _flowRepository.UpdateFlowModel(model);
    }
}
