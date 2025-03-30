namespace MicroFlows.Demo.Models;

public class PaymentReceivedRequest
{
    public string BookingId { get; set; }
    public decimal Amount { get; set; }
}
