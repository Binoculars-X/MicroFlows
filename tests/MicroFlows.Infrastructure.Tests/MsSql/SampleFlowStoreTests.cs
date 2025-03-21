using MicroFlows.Domain.Interfaces;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using Microsoft.Extensions.DependencyInjection;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class MsSqlSampleFlowStoreTests : SqlTestContainersTestBase
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

    public class LinearModel
    {
        public bool FlowStartExecuted { get; set; }
        public bool NextExecuted { get; set; }
        public bool Next2Executed { get; set; }
        public bool FlowEndExecuted { get; set; }
        public bool Condition1 { get; set; }
    }

    public class LinearInlineFlow : FlowBase<LinearModel>
    {
        public override void Define(IFlowBuilder builder)
        {
            builder
                .Begin(() => { Model.FlowStartExecuted = true; })
                .Next(() => { Model.NextExecuted = true; })
                .Next(() => { Model.Next2Executed = true; })
                .End(() => { Model.FlowEndExecuted = true; });
        }
    }
}