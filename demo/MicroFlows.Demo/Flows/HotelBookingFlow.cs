namespace MicroFlows.Demo.Flows;

public class HotelBookingFlow: FlowBase<HotelBookingModel>
{
    public async Task Flow()
    {
        await CallAsync(Init);
    }

    private async Task Init()
    {
        LoadModelFromParams();
        Model.BookingId = Params.ExternalId;
    }
}

public class HotelBookingModel
{
    public string BookingId { get; set; }
    public string VendorId { get; set; }
    public string RoomId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
