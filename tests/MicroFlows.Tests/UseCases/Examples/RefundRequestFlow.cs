using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.Examples;

public class RefundRequestFlow : FlowBase
{
    public const string ApproveReceivedSignal = "Approved";

    public async Task Flow()
    {
        await CallAsync(SubmitRequest);
        await WaitForSignalAsync(ApproveReceivedSignal);
        await CallAsync(ProcessRefund);
    }

    private async Task SubmitRequest()
    {
        // ToDo: implement submit
    }
    private async Task ProcessRefund()
    {
        // ToDo: implement refund
    }
}
