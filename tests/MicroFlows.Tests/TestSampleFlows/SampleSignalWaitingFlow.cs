using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;

public class SampleSignalWaitingFlow : FlowBase
{
    // signals
    public const string Signal1 = "signal1";

    // model consists of all public serializable properties
    public DateTime? ModelDate { get; set; }
    public int? ModelInt { get; set; }

    public async Task Flow()
    {
        Call(Init);

        //var payload = await WaitForSignalAsync<DateTime?>(Signal1);
        await WaitForSignalAsync(Signal1);

        await CallAsync(async() => await Update(DateTime.Now));
    }

    private async Task Update(DateTime? date)
    {
        ModelDate = date;
    }

    private void Init()
    {
        ModelInt = 33;
    }
}
