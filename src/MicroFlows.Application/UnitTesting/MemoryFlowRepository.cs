using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Helpers;
using FluentResults;
using System.Collections.Concurrent;
using JsonPathToModel;
using MicroFlows.Domain.Enums;

namespace MicroFlows.UnitTesting;

public class MemoryFlowRepository : IFlowRepository
{
    internal ConcurrentDictionary<string, FlowStoreModel> _flowModelDictionary = [];

    public Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams)
    {
        var ctx = new FlowContext();
        ctx.Model.ImportFrom(flow, new ImportOptions { ExcludeStartsWith = "__" });
        //ctx.Params = flowParams;
        ctx.RefId = Guid.NewGuid().ToString();
        ctx.ExecutionResult.FlowState = FlowStateEnum.Start;
        ctx.CreatedOn = DateTimeOffset.UtcNow;

        var flowModel = new FlowStoreModel()
        {
            RefId = ctx.RefId,
            ExternalId = flowParams.ExternalId,
            FlowTypeName = flow.GetType().FullName!,
            ContextHistory = [ctx],
            SignalJournal = flow.SignalJournal!,
            Params = flowParams
        };

        _flowModelDictionary[ctx.RefId] = flowModel;
        return Task.FromResult(ctx);
    }

    public Task<FlowStoreModel> UpdateFlow(IFlow flow)
    {
        // merge Flow SignalJournal
        _flowModelDictionary[flow.RefId].SignalJournal.AddRange(flow.SignalJournal);
        var model = _flowModelDictionary[flow.RefId];
        var clone = TypeHelper.CloneObject(model);
        return Task.FromResult(clone);
    }

    public async Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query)
    {
        if (query.RefId != null)
        {
            return await GetFlowHistory(query.RefId);
        }

        if (query.ExternalId != null)
        {
            var record = _flowModelDictionary.Values.FirstOrDefault(f => f.ExternalId == query.ExternalId);
            return record?.ContextHistory;
        }

        return null;
    }

    public Task<List<FlowContext>> GetFlowHistory(string refId)
    {
        var history = _flowModelDictionary[refId].ContextHistory;
        var clone = TypeHelper.CloneObject(history);
        return Task.FromResult(clone);
        //return Task.FromResult(_flowModelDictionary[refId].ContextHistory);
    }

    public Task<FlowStoreModel> GetFlowModel(string refId)
    {
        return Task.FromResult(_flowModelDictionary[refId]);
    }

    public Task SaveContextHistory(List<FlowContext> contextHistory)
    {
        var id = contextHistory.First().RefId;
        _flowModelDictionary[id].ContextHistory = contextHistory;

        // refresh roots
        _flowModelDictionary[id].State = contextHistory.Last().ExecutionResult.FlowState;

        return Task.CompletedTask;
    }

    public async Task<List<FlowStoreModel>> SearchFlowModel(FlowSearchQuery query)
    {
        var result = new List<FlowStoreModel>();
        FlowStoreModel? model = null;

        if (query.RefId != null)
        {
            model = await GetFlowModel(query.RefId);
        }

        if (query.ExternalId != null)
        {
            model = _flowModelDictionary.Values.FirstOrDefault(f => f.ExternalId == query.ExternalId);
        }

        var clone = TypeHelper.CloneObject(model);
        result.Add(clone);

        return result;
    }

    public Task UpdateFlowModel(FlowStoreModel flowModel)
    {
        _flowModelDictionary[flowModel.RefId] = flowModel;
        return Task.CompletedTask;
    }

    //public Task SaveProcessExecutionContext(FlowContext context, TaskExecutionResult executionResult, bool create = false)
    //{
    //    throw new NotImplementedException();
    //}

    public async Task<List<SearchFlowDetails>> SearchFlow(FlowSearchQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<List<FlowInstanceDetails>> GetUnprocessedFlowsWithTimeLock(int batchSize, int timeLock)
    {
        throw new NotImplementedException();
    }

    public async Task<byte[]?> UnlockFlow(FlowInstanceDetails instance)
    {
        return [];
    }

    public Task<FlowExtendedSearchResult> ExtendedSearch(FlowExtendedSearchQuery query)
    {
        throw new NotImplementedException();
    }
 
    public async Task<byte[]?> LockFlow(FlowInstanceDetails instance, int timeLock)
    {
        return [];
    }

    public async Task<byte[]?> AcquireFlowExclusiveLock(FlowInstanceDetails instance, int timeLock)
    {
        return [];
    }
}
