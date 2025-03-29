namespace MicroFlows.Demo.Flows;

public class HotelBookingFlow: FlowBase<HotelBookingModel>
{
    public async Task Flow()
    {
        // use Call or CallAsync to avoid extra execution when replaying
        Call(Init);

        await CallAsync(NotifyBookingReceived);
    }

    private void Init()
    {
        LoadModelFromParams();
    }

    private async Task NotifyBookingReceived()
    {
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

    public HotelBookingStatus Status { get; set; }
}

public enum HotelBookingStatus
{
    Created,
    Approved,
}
