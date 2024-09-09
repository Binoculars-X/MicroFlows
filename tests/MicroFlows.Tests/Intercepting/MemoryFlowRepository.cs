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

namespace MicroFlows.Tests.Intercepting;
internal class MemoryFlowRepository : IFlowRepository
{
    internal ConcurrentDictionary<string, FlowStoreModel> _flowModelDictionary = [];

    public Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams)
    {
        var ctx = new FlowContext();
        ctx.Model.ImportFrom(flow);
        ctx.Params = flowParams;
        ctx.RefId = Guid.NewGuid().ToString();
        ctx.ExecutionResult.FlowState = Domain.Enums.FlowStateEnum.Start;

        var flowModel = new FlowStoreModel()
        {
            RefId = ctx.RefId,
            ExternalId = flowParams.ExternalId,
            FlowTypeName = flow.GetType().FullName!,
            ContextHistory = [ctx],
            SignalJournal = flow.SignalJournal!,
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
        return Task.CompletedTask;
    }
}
