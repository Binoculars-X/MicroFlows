using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;
public interface IFlowRepository
{
    Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams);
    Task<List<FlowContext>> GetFlowHistory(string refId);
    Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query);
    Task<FlowStoreModel> GetFlowModel(string refId);
    Task SaveContextHistory(List<FlowContext> contextHistory);
}

public record FlowSearchQuery(string? RefId, string? ExternalId);
