using MicroFlows.Domain.Interfaces;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;
using MicroFlows.MsSqlRepo;
using Microsoft.Extensions.DependencyInjection;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class MsSqlRepoTests : SqlTestContainersTestBase
{
    [Fact]
    public async Task Can_Create_FlowContext_and_Read_FlowModel()
    {
        var repo = _services.GetService<IFlowRepository>()!;
        var flow = new LinearInlineFlow();
        var ps = new FlowParams();
        var ctx = await repo.CreateFlowContext(flow, ps);

        Assert.NotNull(ctx);

        var flowModel = await repo.GetFlowModel(ctx.RefId);

        Assert.NotNull(flowModel);
        Assert.Single(flowModel.ContextHistory);
    }

    [Fact]
    public async Task Can_Search_Flow_by_ExternalId()
    {
        var repo = _services.GetService<IFlowRepository>()!;
        var flow = new LinearInlineFlow();
        var ps = new FlowParams { ExternalId = "12345" };
        var ctx = await repo.CreateFlowContext(flow, ps);

        Assert.NotNull(ctx);

        var flow2 = new LinearInlineFlow();
        await repo.CreateFlowContext(flow2, ps);

        var models = await repo.SearchFlowModel(new FlowSearchQuery(null, ps.ExternalId));

        Assert.NotNull(models);
        Assert.Equal(2, models.Count);
    }

    [Fact]
    public async Task Can_Search_Flow()
    {
        var repo = _services.GetService<IFlowRepository>() as MsSqlFlowRepository;
        var flow = new LinearInlineFlow();
        var ps = new FlowParams();
        var ctx = await repo.CreateFlowContext(flow, ps);

        Assert.NotNull(ctx);

        var result = await repo.SearchFlow(new FlowSearchQuery(ctx.RefId));

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(Domain.Enums.FlowStateEnum.Start, result.First().State);
        Assert.Equal(Domain.Enums.ResultStateEnum.Success, result.First().Result);
        Assert.Null(result.First().ExternalId);
        Assert.Null(result.First().Tag);
    }

    [Fact]
    public async Task Can_Search_Flow_by_State()
    {
        var repo = _services.GetService<IFlowRepository>() as MsSqlFlowRepository;
        var flow = new LinearInlineFlow();
        var ps = new FlowParams();
        var ctx = await repo.CreateFlowContext(flow, ps);

        Assert.NotNull(ctx);

        var result = await repo.SearchFlow(new FlowSearchQuery(null) { State = Domain.Enums.FlowStateEnum.Start });

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(Domain.Enums.FlowStateEnum.Start, result.First().State);
        Assert.Equal(Domain.Enums.ResultStateEnum.Success, result.First().Result);
        Assert.Null(result.First().ExternalId);
        Assert.Null(result.First().Tag);

        result = await repo.SearchFlow(new FlowSearchQuery(null) { State = Domain.Enums.FlowStateEnum.Stop });

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Can_Search_Flow_by_Tag()
    {
        var repo = _services.GetService<IFlowRepository>() as MsSqlFlowRepository;
        var flow = new LinearInlineFlow();
        var ps = new FlowParams { Tag = "123" };
        await repo.CreateFlowContext(flow, ps);
        await repo.CreateFlowContext(flow, ps);
        ps.Tag = "999";
        await repo.CreateFlowContext(flow, ps);
        
        var result = await repo.SearchFlow(new FlowSearchQuery(null) { Tag = "123" });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}