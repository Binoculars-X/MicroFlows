using Castle.DynamicProxy;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Infrastructure.Tests.Sql.Bases;
using MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.MsSql;

public class MsSqlFlowSearchTests : SqlTestContainersTestBase
{
    [Fact]
    public async Task ExtendedSearch_Returns_Flow()
    {
        var engine = NewEngine();
        var ps = new FlowParams();

        var ctx = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps);

        var result = await _repo.ExtendedSearch(new FlowExtendedSearchQuery());

        Assert.NotNull(result);
        Assert.Single(result.Lines);
        Assert.Equal(typeof(LinearInlineFlow).FullName, result.Lines[0].Name);
        Assert.Null(result.Lines[0].Model);
        Assert.Null(result.Lines[0].Tag);
        Assert.Null(result.Lines[0].ExternalId);
        Assert.Null(result.Lines[0].CorrelationId);
        Assert.True(result.Lines[0].Created < DateTimeOffset.UtcNow);
        Assert.True(result.Lines[0].Modified < DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ExtendedSearch_Includes_Model()
    {
        var engine = NewEngine();
        var ps = new FlowParams();

        var ctx = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps);

        var result = await _repo.ExtendedSearch(new FlowExtendedSearchQuery { IncludeModel = true });

        Assert.NotNull(result);
        Assert.Single(result.Lines);
        Assert.Equal(typeof(LinearInlineFlow).FullName, result.Lines[0].Name);
        Assert.NotNull(result.Lines[0].Model);
    }

    [Fact]
    public async Task ExtendedSearch_Finds_By_CorrelationsId()
    {
        var engine = NewEngine();
        var ps = new FlowParams() { CorrelationId = "123" };
        var ctx = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps);
        var ps2 = new FlowParams() { CorrelationId = "1234" };
        var ctx2 = await engine.ExecuteFlow(typeof(LinearInlineFlow), ps2);

        var result = await _repo.ExtendedSearch(
            new FlowExtendedSearchQuery { CorrelationId = ps.CorrelationId });

        Assert.NotNull(result);
        Assert.Single(result.Lines);
        Assert.Equal(typeof(LinearInlineFlow).FullName, result.Lines[0].Name);
        Assert.Equal(ps.CorrelationId, result.Lines[0].CorrelationId);
    }
}
