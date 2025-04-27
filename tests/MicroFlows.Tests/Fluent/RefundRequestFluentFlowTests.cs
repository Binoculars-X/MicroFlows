using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Tests.Intercepting;
using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.Tests.TestSampleFlows.Fluent;
using MicroFlows.Tests.UseCases.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Fluent;

public class RefundRequestFluentFlowTests : TestBase
{
    [Fact]
    public async Task RefundRequestFluentFlow_Stopped_and_Resumed_after_Timeout()
    {
        var provider = _services.GetService<IFlowProvider>()!;
        var environment = _services.GetService<IFlowTestEnvironment>()!;
        var repo = _services.GetService<IFlowRepository>()!;

        // run flow
        var ps = new FlowParams { FlowName = typeof(RefundRequestFluentFlow).FullName! };
        var ctx = await provider.ExecuteFlow(ps);

        // check flow status
        var flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(FlowStateEnum.Waiting, flow.State);

        // add 3 days to trigger timeout
        environment.MoveTimeForward(TimeSpan.FromDays(3));
        var ps2 = new FlowParams { RefId = ctx.RefId, FlowName = typeof(RefundRequestFluentFlow).FullName! };
        await provider.ExecuteFlow(ps2);

        flow = await repo.GetFlowModel(ctx.RefId);
        Assert.Equal(FlowStateEnum.Finished, flow.State);
    }
}
