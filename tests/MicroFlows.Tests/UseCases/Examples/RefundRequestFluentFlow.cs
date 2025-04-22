using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.Examples;

public class RefundRequestFluentFlow : FlowBase
{
    public const string ApprovalReceivedSignal = "Approved";

    public override void Define(IFlowBuilder builder)
    {
        builder
            .Call(SubmitRequest)
            .WaitForSignalTimeout(ApprovalReceivedSignal, TimeSpan.FromDays(1))
            .If(() => Environment.TimeoutOccurred)
                .Call(RefundFailed)
            .Else()
                .Call(ProcessRefund)
            .EndIf();
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
