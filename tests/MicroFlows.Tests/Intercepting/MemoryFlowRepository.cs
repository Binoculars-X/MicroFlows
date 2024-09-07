using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Helpers;

namespace MicroFlows.Tests.Interceptors;
internal class MemoryFlowRepository : IFlowRepository
{
    internal Dictionary<string, FlowStoreModel> _contextDictHistory = [];

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
        };

        _contextDictHistory[ctx.RefId] = flowModel;
        return Task.FromResult(ctx);
    }

    public Task<List<FlowContext>> GetFlowHistory(string refId)
    {
        return Task.FromResult(_contextDictHistory[refId].ContextHistory);
    }

    public Task<FlowStoreModel> GetFlowModel(string refId)
    {
        return Task.FromResult(_contextDictHistory[refId]);
    }

    public Task SaveContextHistory(List<FlowContext> contextHistory)
    {
        var id = contextHistory.First().RefId;
        _contextDictHistory[id].ContextHistory = contextHistory;
        return Task.CompletedTask;
    }
}
