using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleLoggingFlow : FlowBase
{
    public static List<string> Log = [];

    public string OrderId { get; set; }
    public bool InvoiceSent { get; set; }
    public string SentOrderId { get; set; }
    
    public async Task Flow()
    {
        await CallAsync(GenerateOrderId);

        for (int i = 0; i < 2; i++)
        {
            await CallAsync(async () => await SendMessage(OrderId));
        }

        WaitForCondition(() => InvoiceSent == true);
    }

    private async Task SendMessage(string orderId)
    {
        Log.Add("SendMessage");

        // emulate some external output
        SentOrderId = $"SENT-{orderId}";
    }

    private async Task GenerateOrderId()
    {
        Log.Add("GenerateOrderId");
        OrderId = $"ORDER-{Guid.NewGuid()}";
    }
}
