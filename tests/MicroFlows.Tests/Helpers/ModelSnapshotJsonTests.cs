using JsonPathToModel;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public class ModelSnapshotJsonTests
{
    [Fact]
    public void Any_Json_Can_Be_Prettified()
    {
        var model = new SampleTypedModel { Id = 1, Modified = DateTime.UtcNow, Name = "Bla" };
        var flow = new ModelWrapper<SampleTypedModel> { Model = model };
        var snapshot = new ModelSnapshot();
        snapshot.ImportFrom(flow);

        var json = snapshot.Records["$.Model"].Json;
        var pretty = JsonPrettify(json);
    }

    [Fact]
    public void ModelSnapshot_ToJson_Returns_All_Records()
    {
        var model = new SampleTypedModel { Id = 1, Modified = DateTime.UtcNow, Name = "Bla" };
        var flow = new MyModel { Model = model, Column1 = "text" };
        var snapshot = new ModelSnapshot();
        snapshot.ImportFrom(flow);

        var json = snapshot.ToJson();

        Assert.Contains("  \"Column1\": \"text\"", json);
        Assert.Contains("  \"Model\": {", json);
        Assert.Contains("    \"Id\": 1,", json);
        Assert.Contains("    \"Modified\": ", json);
        Assert.Contains("    \"Name\": \"Bla\",", json);
    }

    public string JsonPrettify(string json)
    {
        using var jDoc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
        return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
    }

    public class MyModel : ModelWrapper<SampleTypedModel>
    {
        public string Column1 { get; set; }
    }
}
