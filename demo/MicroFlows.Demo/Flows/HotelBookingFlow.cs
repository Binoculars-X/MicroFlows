using MicroFlows.Demo.Models;

namespace MicroFlows.Demo.Flows;

public class HotelBookingFlow: FlowBase<HotelBookingModel>
{
    // signals
    public const string PaymentReceivedSignal = "PaymentReceived";
    public const string ReservationConfirmedSignal = "ReservationConfirmed";

    public override void SetSignalHandlers()
    {
        AddSignalHandler(ReservationConfirmedSignal, ReservationReceivedHandler);
    }

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
                await CallAsync(NotifyPaymentFailed);
                await CallAsync(NotifyBookingFailed);
                return;
            }
        }
        else
        {
            await WaitForSignalTimeoutAsync(PaymentReceivedSignal, TimeSpan.FromDays(3));

            if (Environment.TimeoutOccurred)
            {
                // timeout reached
                await CallAsync(NotifyPaymentFailed);
                await CallAsync(NotifyBookingFailed);
                return;
            }
        }

        await WaitForSignalTimeoutAsync(ReservationConfirmedSignal, TimeSpan.FromDays(1));

        if (Environment.TimeoutOccurred || Model.ReservationFailed)
        {
            await CallAsync(RevertPayment);
            await CallAsync(NotifyBookingFailed);
            return;
        }

        await CallAsync(FinalizeBooking);
        await CallAsync(NotifyBookingCompleted);
    }

    private Task ReservationReceivedHandler(SignalPayload payload)
    {
        Model.ReservationPayload = payload.GetValue<ReservationReceivedPayload>();
        Model.ReservationFailed = !Model.ReservationPayload.Success;
        return Task.CompletedTask;
    }

    private void Init()
    {
        LoadModelFromParams();
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
    }
    private async Task RevertPayment()
    {
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
    private async Task NotifyPaymentFailed()
    {
    }
}

