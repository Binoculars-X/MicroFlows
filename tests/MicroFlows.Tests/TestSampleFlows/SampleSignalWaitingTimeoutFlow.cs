using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;

public class SampleSignalWaitingTimeoutFlow : FlowBase
{
    // signals
    public const string Signal1 = "signal1";

    // model consists of all public serializable properties
    public DateTime? Signal1PayloadDate { get; set; }
    public DateTime? ModelDate { get; set; }
    public int? ModelInt { get; set; }
    public bool? Timeout1 { get; set; }
    public bool? Timeout2 { get; set; }

    public async Task Flow()
    {
        AddSignalHandler(Signal1, Signal1Handler);

        //await CallAsync(Init);

        // pass first time out
        await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(0));
        Timeout1 = TimeoutOccurred;

        // stop here
        await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(2));
        Timeout2 = TimeoutOccurred;

        await CallAsync(async () => await Update(DateTime.Now));
    }

    private Task Signal1Handler(SignalPayload payload)
    {
        Signal1PayloadDate = payload.GetValue<DateTime?>();
        return Task.CompletedTask;
    }

    private async Task Update(DateTime? date)
    {
        ModelDate = date;
    }

    private async Task Init()
    {
        ModelInt = 33;
    }
}