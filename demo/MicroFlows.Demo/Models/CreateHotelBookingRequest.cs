using MicroFlows.Demo.Flows;

namespace MicroFlows.Demo.Models;

public class CreateHotelBookingRequest
{
    public string BookingId { get; set; }
    public string VendorId { get; set; }
    public string RoomId { get; set; }
    public string CustomerId { get; set; }
    public DateTime Created { get; set; }
    public DateTime From { get; set; }
    public int StayDays { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal Amount { get; set; }
}
    