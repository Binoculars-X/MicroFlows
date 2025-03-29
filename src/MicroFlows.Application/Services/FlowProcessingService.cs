using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application.Services;

public class FlowProcessingService : IFlowProcessingService
{
    public FlowProcessingService(FlowProcessingServiceSettings settings)
    {

    }

    public FlowProcessingService(IOptions<FlowProcessingServiceSettings> settings)
    {

    }

    public async Task ExecuteIteration()
    {
    }
}

public record FlowProcessingServiceSettings()
{
    public int BatchSize = 1000;
}
