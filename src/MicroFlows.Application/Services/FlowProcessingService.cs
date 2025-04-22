using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application.Services;

public class FlowProcessingService : IFlowProcessingService
{
    public const int BATCH_SIZE = 100;
    public const int TIME_LOCK_MILLISECONDS = 1000;
    public const int MAX_PARALLELISM = 10;

    private readonly IFlowProvider _flowProvider;
    private readonly IFlowRepository _flowRepository;
    //public FlowProcessingService(FlowProcessingServiceSettings settings)
    //{

    //}

    public FlowProcessingService(IOptions<FlowProcessingServiceSettings> settings,
        IFlowRepository flowRepository,
        IFlowProvider flowProvider)
    {
        _flowRepository = flowRepository;
        _flowProvider = flowProvider;
    }

    public async Task ExecuteIteration()
    {
        var flows = await _flowRepository.GetUnprocessedFlowsWithTimeLock(BATCH_SIZE, TIME_LOCK_MILLISECONDS);
        var options = new ParallelOptions { MaxDegreeOfParallelism = MAX_PARALLELISM };

        await Parallel.ForEachAsync(flows, options, async (flow, cancellationToken) =>
        {
            var ps = new FlowParams { RefId = flow.RefId, FlowName = flow.FlowName };

            try
            {
                await _flowProvider.ExecuteFlow(ps, cancellationToken);
            }
            finally
            {
                await _flowRepository.UnlockFlow(flow);
            }
        });
    }
}

public record FlowProcessingServiceSettings()
{
    public int BatchSize = 1000;
}
