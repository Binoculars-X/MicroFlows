using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Intercepting;

public class ModelSnapshotTests : TestBase
{
    readonly MemoryFlowRepository _repo;

    public ModelSnapshotTests()
    {
        _repo = new MemoryFlowRepository();
    }

    private FlowEngine GetEngine()
    {
        return new FlowEngine(new NullLogger<FlowEngine>(),
            _services,
            new ProxyGenerator(),
            _repo);
    }

    [Fact]
    public async Task SeModelFromParams_Sets_Model_Successfully()
    {
        var model = new SampleTypedModel { Id = 10, Name = "Kaha", Modified = DateTime.Now };
        var ps = FlowParams.CreateWithPayload(model);

        Assert.NotNull(ps?.Payload);

        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(TypedModelFlow), ps);

        Assert.NotNull(ctx);
        Assert.NotNull(ctx.Model);

        var result = new TypedModelFlow();
        ctx.Model.ExportTo(result);

        Assert.Equal(model.Id, result.Model.Id);
        Assert.Equal(model.Name, result.Model.Name);
        Assert.Equal(model.Modified, result.Model.Modified);
    }

    [Fact]
    public async Task SeModelFromParams_Sets_Model_Successfully_For_InlineFlow()
    {
        var model = new SampleTypedModel { Id = 10, Name = "Kaha", Modified = DateTime.Now };
        var ps = FlowParams.CreateWithPayload(model);

        Assert.NotNull(ps?.Payload);

        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(TypedModelInlineFlow), ps);

        Assert.NotNull(ctx);
        Assert.NotNull(ctx.Model);

        var result = new TypedModelInlineFlow();
        ctx.Model.ExportTo(result);

        Assert.Equal(model.Id, result.Model.Id);
        Assert.Equal(model.Name, result.Model.Name);
        Assert.Equal(model.Modified, result.Model.Modified);
    }

    [Fact]
    public async Task SeModelFromParams_Sets_Model_Successfully_For_UntypedModelFlow()
    {
        var model = new SampleTypedModel { Id = 10, Name = "Kaha", Modified = DateTime.Now };
        var ps = FlowParams.CreateWithPayload(model);

        Assert.NotNull(ps?.Payload);

        var engine = GetEngine();
        var ctx = await engine.ExecuteFlow(typeof(UntypedModelInlineFlow), ps);

        Assert.NotNull(ctx);
        Assert.NotNull(ctx.Model);

        var result = new UntypedModelInlineFlow();
        ctx.Model.ExportTo(result);

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Modified, result.Modified);
    }

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

    public class TypedModelFlow : FlowBase<SampleTypedModel>
    {
        public async Task Flow()
        {
            await CallAsync(Init);
        }

        private async Task Init()
        {
            LoadModelFromParams();
        }
    }

    public class TypedModelInlineFlow : FlowBase<SampleTypedModel>
    {
        public async Task Flow()
        {
            LoadModelFromParams();
        }
    }

    public class UntypedModelInlineFlow : FlowBase
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Modified { get; set; }

        public async Task Flow()
        {
            LoadModelFromParams();
        }
    }
}
