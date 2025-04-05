using MicroFlows.Demo.Models;

namespace MicroFlows.Demo.Flows;

public class HotelBookingFlow: FlowBase<HotelBookingModel>
{
    // signals
    public const string PaymentReceivedSignal = "PaymentReceived";
    public const string ReservationConfirmedSignal = "ReservationConfirmed";

    public async Task Flow()
    {
        // use Call or CallAsync to avoid extra execution when replaying
        Call(Init);

        await CallAsync(NotifyBookingReceived);

        await CallAsync(VendorReservation);

        if (Model.PaymentType == PaymentType.Card)
        {
            await CallAsync(ProcessCardPayment);

            if (Model.PaymentFailed)
            {
                await CallAsync(VendorReservationReverse);

                await CallAsync(NotifyBookingFailed);
                
                return;
            }
        }

        if (Model.PaymentType != PaymentType.Card)
        {
            if(!await WaitForSignalTimeoutAsync(PaymentReceivedSignal, TimeSpan.FromDays(3)))
            {
                // timeout reached
                await CallAsync(NotifyBookingFailed);

                return;
            }
        }

        await WaitForSignalAsync(ReservationConfirmedSignal);

        await CallAsync(FinalizeBooking);

        await CallAsync(NotifyBookingCompleted);
    }

    private void Init()
    {
        LoadModelFromParams();
        Model.Status = HotelBookingStatus.Created;
    }

    private async Task NotifyBookingReceived()
    {
    }
    private async Task VendorReservation()
    {
    }
    private async Task VendorReservationReverse()
    {
    }
    private async Task ProcessCardPayment()
    {
        Model.PaymentFailed = true;
    }
    private async Task FinalizeBooking()
    {
        // start booking order support flow
    }
    private async Task NotifyBookingCompleted()
    {
    }
    private async Task NotifyBookingFailed()
    {
    }
}

