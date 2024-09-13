using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Intercepting;

public class ModelSnapshotTests
{
    [Fact]
    public void ModelSnapshot_Should_IgnoreProxySystemFields()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var flow = new SampleFlow()
        {
            Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        var flowProxy = new ProxyGenerator().CreateClassProxyWithTarget(classToProxy: flow.GetType(),
                constructorArguments: null,
                target: flow,
                options: new ProxyGenerationOptions(),
                interceptors: []) as FlowBase;

        model.ImportFrom(flowProxy);
        Assert.Equal(6, model.Records.Count);
        Assert.True(model.Records.ContainsKey("$.__interceptors"));
        Assert.True(model.Records.ContainsKey("$.__target"));

        model.ImportFrom(flowProxy, new ImportOptions { ExcludeStartsWith = "__" });
        Assert.Equal(4, model.Records.Count);
        Assert.False(model.Records.ContainsKey("$.__interceptors"));
        Assert.False(model.Records.ContainsKey("$.__target"));
    }
}
