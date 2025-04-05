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
    public bool? Timeout1Reached { get; set; }
    public bool? Timeout2Reached { get; set; }

    public async Task Flow()
    {
        AddSignalHandler(Signal1, Signal1Handler);

        //await CallAsync(Init);

        // pass first time out
        Timeout1Reached = await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(0));

        // stop here
        Timeout2Reached = await WaitForSignalTimeoutAsync(Signal1, TimeSpan.FromSeconds(2));

        //Timeout1Reached = WaitForSignal(Signal1, TimeSpan.FromSeconds(0));
        //Timeout2Reached = WaitForSignal(Signal1, TimeSpan.FromSeconds(2));

        await CallAsync(async() => await Update(DateTime.Now));
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
