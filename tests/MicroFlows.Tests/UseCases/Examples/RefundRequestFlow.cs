using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.Examples;

public class RefundRequestFlow : FlowBase
{
    public const string ApprovalReceivedSignal = "Approved";

    public async Task Flow()
    {
        await CallAsync(SubmitRequest);
        await WaitForSignalTimeoutAsync(ApprovalReceivedSignal, TimeSpan.FromDays(1));

        if (Environment.TimeoutOccurred)
        {
            await CallAsync(RefundFailed);
        }
        else
        {
            await CallAsync(ProcessRefund);
        }
    }

    private async Task SubmitRequest()
    {
        // ToDo: implement submit
    }
    private async Task ProcessRefund()
    {
        // ToDo: implement refund
    }
    private async Task RefundFailed()
    {
        // ToDo: implement refund failed
    }
}
