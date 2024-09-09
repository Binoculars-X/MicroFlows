using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;

public class SampleTwoSignalPayloadWaitingFlow : FlowBase
{
    // signals
    public const string Signal1 = "signal1";
    public const string Signal2 = "signal2";

    // model consists of all public serializable properties
    public DateTime? Signal1PayloadDate { get; set; }
    public string? Signal2PayloadText { get; set; }
    public DateTime? ModelDate { get; set; }
    public int? ModelInt { get; set; }

    public async Task Flow()
    {
        AddSignalHandler(Signal1, Signal1Handler);
        AddSignalHandler(Signal2, Signal2Handler);

        Call(Init);

        //var payload = await WaitForSignalAsync<DateTime?>(Signal1);
        await WaitForSignalAsync(Signal1);
        await WaitForSignalAsync(Signal2);

        await CallAsync(async() => await Compare(Signal1PayloadDate));
    }

    private Task Signal1Handler(SignalPayload payload)
    {
        Signal1PayloadDate = payload.GetValue<DateTime?>();
        return Task.CompletedTask;
    }

    private Task Signal2Handler(SignalPayload payload)
    {
        Signal2PayloadText = payload.GetValue<string>();
        return Task.CompletedTask;
    }

    private async Task Compare(DateTime? date)
    {
        if (ModelDate == date)
        {
        }
    }

    private void Init()
    {
        ModelInt = 33;
    }
}
