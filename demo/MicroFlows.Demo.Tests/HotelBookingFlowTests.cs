using MicroFlows.Demo.Flows;
using MicroFlows.Demo.Models;
using MicroFlows.Demo.Tests.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MicroFlows.Demo.Tests;

public class HotelBookingFlowTests
{
    [Fact]
    public async Task HotelBookingFlow_Stopped_and_Resumed_after_Timeout()
    {
        // create app 
        var servivces = TestServices.CreateInMemory();
        var provider = servivces.GetService<IFlowProvider>()!;
        var environment = servivces.GetService<IFlowTestEnvironment>()!;
        var repo = servivces.GetService<IFlowRepository>()!;

        // run flow
        var req = new CreateHotelBookingRequest { BookingId = "BK0001" };
        var ps = FlowParams.CreateWithPayload(req);
        ps.FlowName = typeof(HotelBookingFlow).FullName!;
        ps.ExternalId = req.BookingId;
        var ctx = await provider.ExecuteFlow(ps);

        // check flow status
        var flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(FlowStateEnum.Waiting, flow.State);

        // add 3 days to trigger timeout
        environment.MoveTimeForward(TimeSpan.FromDays(3));
        var ps2 = new FlowParams { RefId = ctx.RefId, FlowName = typeof(HotelBookingFlow).FullName! };
        await provider.ExecuteFlow(ps2);

        flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(FlowStateEnum.Finished, flow.State);
    }
}
