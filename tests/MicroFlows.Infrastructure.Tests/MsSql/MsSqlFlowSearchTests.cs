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
        Assert.Single(result);
        Assert.Equal(typeof(LinearInlineFlow).FullName, result[0].Name);
        Assert.Null(result[0].Model);
        Assert.Null(result[0].Tag);
        Assert.Null(result[0].ExternalId);
        Assert.Null(result[0].CorrelationId);
        Assert.True(result[0].Created < DateTimeOffset.UtcNow);
        Assert.True(result[0].Modified < DateTimeOffset.UtcNow);
    }
    
}
