using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;

/// <summary>
/// Provides function to make environment corrections like setting Date in future to bypass Waiting methods
/// </summary>
public interface IFlowTestEnvironment
{
    FlowTestEnvironmentDetails GetFlowTestEnvironment(string flow, string refId, string externalId);
}

public record FlowTestEnvironmentDetails()
{
    public TimeSpan? CurrentDateTimeCorrection { get; set; }

    public static FlowTestEnvironmentDetails EmptyProd()
    {
        return new FlowTestEnvironmentDetails();
    }
}
