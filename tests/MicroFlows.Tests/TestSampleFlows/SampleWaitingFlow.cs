using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleWaitingFlow : FlowBase
{
    public string OrderId { get; set; }
    public bool InvoiceSent { get; set; }
    public string SentOrderId { get; set; }
    
    public async Task Flow()
    {
        await CallAsync(GenerateOrderId);
        await CallAsync(async () => await SendMessage(OrderId));
        WaitForCondition(() => InvoiceSent == true);
    }

    private async Task SendMessage(string orderId)
    {
        // emulate some external output
        SentOrderId = $"SENT-{orderId}";
    }

    private async Task GenerateOrderId()
    {
        OrderId = $"ORDER-{Guid.NewGuid()}";
    }
}
