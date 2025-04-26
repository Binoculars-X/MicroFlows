using MicroFlows.Demo.Flows;
using MicroFlows.Demo.Models;
using MicroFlows.Demo.Tests.Helpers;
using MicroFlows.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace MicroFlows.Demo.Tests;

public class HotelBookingFlowSqlTests : IAsyncLifetime
{
    protected readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
        => _msSqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task HotelBookingFlow_Model_Populated_from_Parameters_SqlServer()
    {
        // create app 
        var servivces = TestServices.CreateSql(_msSqlContainer.GetConnectionString());
        var provider = servivces.GetService<IFlowProvider>()!;
        var repo = servivces.GetService<IFlowRepository>()!;

        // run flow
        var req = new CreateHotelBookingRequest { BookingId = "BK0001", Amount = 100, RoomId = "#123" };
        var ps = FlowParams.CreateWithPayload(req);
        ps.FlowName = typeof(HotelBookingFlow).FullName!;
        ps.ExternalId = req.BookingId;
        var ctx = await provider.ExecuteFlow(ps);

        // check flow model
        var flow = await repo.GetFlowModel(ctx.RefId);
        var flowModel = flow.ContextHistory.ElementAt(1).Model.Deserialize<HotelBookingModel>();
        Assert.Equal(req.BookingId, flowModel.BookingId);
        Assert.Equal(req.Amount, flowModel.Amount);
        Assert.Equal(req.RoomId, flowModel.RoomId);

        // check flow status
        Assert.Equal(FlowStateEnum.Waiting, flow.State);
    }
}
