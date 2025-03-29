using MicroFlows.Domain.Interfaces;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class MsSqlRepoSettingsTests: SqlTestContainersTestBase
{
    public override void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddMicroFlowsMsSqlRepo(configuration,
                    new MsSqlFlowRepositorySettings
                    {
                        ConnectionString = _msSqlContainer.GetConnectionString(),
                        CreateDatabase = true,
                        DatabaseName = "Test1"
                    });
    }

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
}
