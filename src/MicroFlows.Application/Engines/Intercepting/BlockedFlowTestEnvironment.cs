using MicroFlows.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application.Engines.Intercepting;

public class BlockedFlowTestEnvironment : IFlowTestEnvironment
{
    public FlowTestEnvironmentDetails GetFlowTestEnvironment(string flow, string refId, string externalId)
    {
        return FlowTestEnvironmentDetails.EmptyProd();
    }
}
