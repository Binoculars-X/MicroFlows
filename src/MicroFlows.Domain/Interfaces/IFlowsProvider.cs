using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public interface IFlowsProvider
{
    Task<FlowContext> ExecuteFlow(Type flowType, FlowParams? flowParams = null);
    Task<FlowContext> ExecuteFlow(string flowTypeName, FlowParams? flowParams = null);
}
