using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public interface IFlowRepository
{
    Task<FlowStoreModel> GetFlowModel(string refId);
    Task<List<FlowContext>> GetFlowHistory(string refId);
    Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query);
    Task<List<FlowStoreModel>> SearchFlowModel(FlowSearchQuery query);
    Task<List<FlowExtendedSearchResult>> ExtendedSearch(FlowExtendedSearchQuery query);

    Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams);
    Task<FlowStoreModel> UpdateFlow(IFlow flow);
    Task UpdateFlowModel(FlowStoreModel flowModel);

    Task SaveContextHistory(List<FlowContext> contextHistory);

    Task<List<SearchFlowDetails>> SearchFlow(FlowSearchQuery query);
    Task<List<FlowInstanceDetails>> GetUnprocessedFlowsWithTimeLock(int batchSize, int timeLock);
    Task<bool> LockFlow(FlowInstanceDetails instance, int timeLock);
    Task<bool> UnlockFlow(FlowInstanceDetails instance);
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

public record FlowExtendedSearchResult(
    string? RefId,
    string? ExternalId,
    string? CorrelationId,
    FlowStateEnum? Status,
    string? Name,
    string? Tag,
    DateTime? Created,
    DateTime? Modified,
    FlowStoreModel? Model);

public enum FlowSearchSortOrder
{
    RefId,
    ExternalId,
    CorrelationId,
    Status,
    Name,
    Tag,
    Created,
    Modified
}

public record FlowExtendedSearchQuery()
{
    public string? RefId;
    public string? ExternalId;
    public string? CorrelationId;
    public FlowStateEnum? Status;
    public string? Name;
    public string? Tag;
    public DateTime? CreatedFrom;
    public DateTime? ModifiedFrom;

    public FlowSearchSortOrder? Sort;
    public bool? Ascending;

    public bool? IncludeModel;
    public int Take = 200;

    public bool IsNotEmpty()
    {
        return !string.IsNullOrEmpty(RefId) || !string.IsNullOrEmpty(ExternalId) || !string.IsNullOrEmpty(CorrelationId)
            || !string.IsNullOrEmpty(Tag) || Status != null || string.IsNullOrEmpty(Name)
            || CreatedFrom != null || ModifiedFrom != null;
    }

    public bool IsSorting()
    {
        return Sort != null;
    }
}

