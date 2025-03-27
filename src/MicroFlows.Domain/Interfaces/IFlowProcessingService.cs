using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;

/// <summary>
/// Manages recurring flow executions
/// </summary>
public interface IFlowProcessingService
{
    /// <summary>
    /// Process iteration, re-running a bunch of flows. 
    /// Must be run periodically by a background job (for example: Hangfire or Quartz).
    /// </summary>
    /// <returns></returns>
    Task ExecuteIteration();
}
