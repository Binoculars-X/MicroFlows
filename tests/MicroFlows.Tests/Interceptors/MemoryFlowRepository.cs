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
    Dictionary<string, List<FlowContext>> _contextDictHistory = [];

    public async Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams)
    {
        var ctx = new FlowContext();
        ctx.Model.ImportFrom(flow);
        ctx.Params = flowParams;
        ctx.RefId = Guid.NewGuid().ToString();
        _contextDictHistory[ctx.RefId] = [ctx];
        return ctx;
    }

    public async Task<List<FlowContext>> GetFlowHistory(string refId)
    {
        return _contextDictHistory[refId];
    }

    public async Task SaveContextHistory(List<FlowContext> contextHistory)
    {
        var id = contextHistory.First().RefId;
        _contextDictHistory[id] = contextHistory;
    }
}
