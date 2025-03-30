using MicroFlows.Demo.Flows;
using MicroFlows.Demo.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroFlows.Demo.Controllers
{
    [ApiController]
    [Route("api/v1/hotelbooking")]
    public class HotelBookingController : ControllerBase
    {
        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly ILogger<HotelBookingController> _logger;
        private readonly IFlowProvider _flowProvider;

        public HotelBookingController(ILogger<HotelBookingController> logger, IFlowProvider flowProvider)
        {
            _logger = logger;
            _flowProvider = flowProvider;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [HttpPost()]
        public async Task<ActionResult<string>> PostAsync([FromBody] CreateHotelBookingRequest req, 
            CancellationToken cancellationToken)
        {
            var ps = FlowParams.CreateWithPayload(req);
            ps.FlowName = typeof(HotelBookingFlow).FullName!;
            ps.ExternalId = req.BookingId;
            var ctx = await _flowProvider.CreateFlow(ps, cancellationToken);

            // ToDo: remove when Server start processing in background
            var ps2 = new FlowParams { RefId = ctx.RefId, FlowName = typeof(HotelBookingFlow).FullName! };
            await _flowProvider.ExecuteFlow(ps2, cancellationToken);

            return Ok(ctx.RefId);
        }

        [HttpPost("payment-received")]
        public async Task<ActionResult> PostPaymentReceivedAsync([FromBody] PaymentReceivedRequest req, 
            CancellationToken cancellationToken)
        {
            var ps = new FlowParams();
            ps.FlowName = typeof(HotelBookingFlow).FullName!;
            ps.ExternalId = req.BookingId;
            await _flowProvider.SendSignal(ps, HotelBookingFlow.PaymentReceivedSignal);
            return Ok();
        }

        [HttpPost("confirm-reservation")]
        public async Task<ActionResult> PostConfirmReservationAsync([FromBody] ConfirmReservationRequest req,
            CancellationToken cancellationToken)
        {
            var ps = new FlowParams();
            ps.FlowName = typeof(HotelBookingFlow).FullName!;
            ps.ExternalId = req.BookingId;
            await _flowProvider.SendSignal(ps, HotelBookingFlow.ReservationConfirmedSignal);
            return Ok();
        }
    }
}
