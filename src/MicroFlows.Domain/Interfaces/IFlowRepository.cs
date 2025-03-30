using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;

public interface IFlowRepository
{
    Task<FlowStoreModel> GetFlowModel(string refId);
    Task<List<FlowContext>> GetFlowHistory(string refId);
    Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query);
    Task<List<FlowStoreModel>> SearchFlowModel(FlowSearchQuery query);

    Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams);
    Task<FlowStoreModel> UpdateFlow(IFlow flow);
    Task UpdateFlowModel(FlowStoreModel flowModel);

    Task SaveContextHistory(List<FlowContext> contextHistory);

    Task<List<SearchFlowDetails>> SearchFlow(FlowSearchQuery query);
    Task<List<FlowInstanceDetails>> GetUnprocessedFlowsWithTimeLock(int batchSize, int timeLock);
}

public record FlowSearchQuery(string? RefId, string? ExternalId = null)
{
    public string? Tag;
    public FlowStateEnum? State;
    public ResultStateEnum? Result;

    public bool IsNotEmpty()
    {
        return !string.IsNullOrEmpty(RefId) || !string.IsNullOrEmpty(ExternalId) 
            || !string.IsNullOrEmpty(Tag) || State != null || Result != null;
    }
}

public record FlowInstanceDetails(string RefId, string FlowName, byte[] Version);
