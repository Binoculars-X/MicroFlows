using MicroFlows.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.UnitTesting;

public class IntegrationFlowTestEnvironment : IFlowTestEnvironment
{
    private FlowTestEnvironmentDetails _details = new();

    public FlowTestEnvironmentDetails GetFlowTestEnvironment(string flow, string refId, string externalId)
    {
        return _details;
    }

    public void MoveTimeForward(TimeSpan timeSpan)
    {
        _details.CurrentDateTimeCorrection = timeSpan;
    }
}
