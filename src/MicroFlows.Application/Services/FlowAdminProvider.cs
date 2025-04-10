using JsonPathToModel;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application.Services;

public class FlowAdminProvider : IFlowAdminProvider
{
    private readonly IFlowRepository _flowRepository;

    public FlowAdminProvider(IFlowRepository flowRepository)
    {
        _flowRepository = flowRepository;
    }

    public async Task DeleteExecutionSteps(string RefId, int index)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.ContextHistory.RemoveRange(index, model.ContextHistory.Count - index);
        var flow = new FlowInstanceDetails(model.RefId, model.FlowTypeName, model.Timestamp);

        // Flow can be locked, we need to wait until it is released
        try
        {
            model.Timestamp = await _flowRepository.AcquireFlowExlusiveLock(flow);
            await _flowRepository.UpdateFlowModel(model);
        }
        finally
        {
            await _flowRepository.UnlockFlow(flow);
        }
    }

    public async Task UpdateFlowContextModel(string RefId, int index, ModelSnapshot updated)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.ContextHistory[index].Model = updated;
        await _flowRepository.UpdateFlowModel(model);
    }

    public async Task UpdateFlowSignals(string RefId, List<SignalJournalEntry> updated)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.SignalJournal = updated;
        await _flowRepository.UpdateFlowModel(model);
    }

    public async Task UpdateFlowContextParams(string RefId, int index, FlowParams updated)
    { 
    }

    public async Task UpdateFlowStatus(string RefId, FlowStateEnum status)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.State = status;
        await _flowRepository.UpdateFlowModel(model);
    }

    public async Task HaltFlow(string RefId)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.State = FlowStateEnum.Halt;
        var flow = new FlowInstanceDetails(model.RefId, model.FlowTypeName, model.Timestamp);
        
        // Flow can be locked, we need to wait until it is released
        try
        {
            model.Timestamp = await _flowRepository.AcquireFlowExlusiveLock(flow);
            await _flowRepository.UpdateFlowModel(model);
        }
        finally
        {
            await _flowRepository.UnlockFlow(flow);
        }
    }

    public async Task RerunFlow(string RefId)
    {
        // Flow should be stopped at this point, so no lock needed
        var model = await _flowRepository.GetFlowModel(RefId);
        model.State = FlowStateEnum.Rerun;
        await _flowRepository.UpdateFlowModel(model);
    }
}
