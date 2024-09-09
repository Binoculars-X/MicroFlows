using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;

public class SampleCheckSignalFlow : FlowBase
{
    // signals
    public const string OrderAcceptedSignal = "OrderAccpetedSignal";
    public const string OrderCancelledSignal = "OrderCancelledSignal";

    // model consists of all public serializable properties
    public string OrderId { get; set; }
    public DateTime? CancelDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public string OrderStatus { get; set; }

    public async Task Flow()
    {
        AddSignalHandler(OrderCancelledSignal, OrderCancelledSignalHandler);

        Call(Init);

        await CheckSignalReceivedAsync(OrderCancelledSignal);
        await WaitForSignalAsync(OrderAcceptedSignal);
        await CheckSignalReceivedAsync(OrderCancelledSignal);

        await CallAsync(Finish);
    }

    private Task OrderCancelledSignalHandler(SignalPayload payload)
    {
        CancelDate = payload.GetValue<DateTime?>();
        return Task.CompletedTask;
    }

    private async Task Finish()
    {
        if (CancelDate != null)
        {
            OrderStatus = "Cancelled";
        }
        else
        {
            OrderStatus = "Processed";
            FinishDate = DateTime.UtcNow;
        }
    }

    private void Init()
    {
        OrderStatus = "Generated";
    }
}
