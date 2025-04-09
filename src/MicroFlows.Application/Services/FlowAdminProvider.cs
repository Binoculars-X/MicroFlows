using JsonPathToModel;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public async Task UpdateFlowContextModel(string RefId, int index, ModelSnapshot model)
    {
    }

    public async Task UpdateFlowSignals(string RefId, List<SignalJournalEntry> journal)
    {
    }

    public async Task UpdateFlowStatus(string RefId, FlowStateEnum status)
    {
        var model = await _flowRepository.GetFlowModel(RefId);
        model.State = status;
        await _flowRepository.UpdateFlowModel(model);
    }
}
