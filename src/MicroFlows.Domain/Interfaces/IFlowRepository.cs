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
    Task<FlowExtendedSearchResult> ExtendedSearch(FlowExtendedSearchQuery query);

    Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams);
    Task<FlowStoreModel> UpdateFlow(IFlow flow);
    Task UpdateFlowModel(FlowStoreModel flowModel);

    Task SaveContextHistory(List<FlowContext> contextHistory);

    Task<List<SearchFlowDetails>> SearchFlow(FlowSearchQuery query);
    Task<List<FlowInstanceDetails>> GetUnprocessedFlowsWithTimeLock(int batchSize, int timeLock);
    Task<byte[]?> LockFlow(FlowInstanceDetails instance, int timeLock);
    Task<byte[]?> UnlockFlow(FlowInstanceDetails instance);
    Task<byte[]?> AcquireFlowExlusiveLock(FlowInstanceDetails instance, int timeLock = 0);
}

public record FlowSearchQuery(string? RefId, string? ExternalId = null)
{
    public string? Tag;
    public FlowStateEnum? Status;
    public ResultStateEnum? Result;

    public bool IsNotEmpty()
    {
        return !string.IsNullOrEmpty(RefId) || !string.IsNullOrEmpty(ExternalId) 
            || !string.IsNullOrEmpty(Tag) || Status != null || Result != null;
    }
}

public record FlowInstanceDetails(string RefId, string FlowName, byte[] Version);

public record FlowExtendedSearchResult(List<FlowExtendedSearchResultLine> Lines, int Count);

public record FlowExtendedSearchResultLine(
    string RefId,
    string? ExternalId,
    string? CorrelationId,
    FlowStateEnum? Status,
    string? Task,
    string Name,
    string? Tag,
    DateTimeOffset? Created,
    DateTimeOffset? Modified,
    FlowStoreModel? Model);

public enum FlowSearchSortOrder
{
    RefId,
    ExternalId,
    CorrelationId,
    Status,
    Task,
    Name,
    Tag,
    Created,
    Modified
}

public record FlowExtendedSearchQuery()
{
    public string? RefId { get; set; }
    public string? ExternalId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Status { get; set; }
    public string? Name { get; set; }
    public string? Tag { get; set; }
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? ModifiedFrom { get; set; }

    public FlowSearchSortOrder? Sort { get; set; }
    public bool? Ascending { get; set; }

    public bool? IncludeModel;
    public int Take = 200;

    public bool IsNotEmpty()
    {
        return !string.IsNullOrEmpty(RefId) || !string.IsNullOrEmpty(ExternalId) || !string.IsNullOrEmpty(CorrelationId)
            || !string.IsNullOrEmpty(Tag) || Status != null || !string.IsNullOrEmpty(Name)
            || CreatedFrom != null || ModifiedFrom != null;
    }

    public bool IsSorting()
    {
        return Sort != null;
    }
}

