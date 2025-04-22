using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.Examples;

public class RefundRequestFluentFlow : FlowBase
{
    public const string ApproveReceivedSignal = "Approved";

    public override void Define(IFlowBuilder builder)
    {
        builder
            .Call(SubmitRequest)
            .WaitForSignal(ApproveReceivedSignal)
            .Call(ProcessRefund);
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
