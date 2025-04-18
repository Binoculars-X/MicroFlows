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
    public void Snapshot_Record_Value_Can_Be_Updated()
    {
        var model = new SampleTypedModel { Id = 1, Modified = DateTime.UtcNow, Name = "Bla" };

        var m2 = new MyModel2 { Model = model, Bool1 = false, DateTimeOffset1 = DateTimeOffset.Now, 
            Decimal1 = 11, Decimal2 = 100 };

        var snapshot = new ModelSnapshot();
        snapshot.ImportFrom(m2);

        var json = snapshot.Records["$.Model"].Json.JsonPrettify();
        var b1 = snapshot.Records["$.Bool1"].Json;
        var d1 = snapshot.Records["$.Decimal1"].Json;
        var dto1 = snapshot.Records["$.DateTimeOffset1"].Json;

        Assert.Equal("false", b1);
        Assert.Equal("11", d1);

        b1 = "true";
        d1 = "127";
        dto1 = "\"2025-04-18T09:39:39.0000001+07:00\"";
        json = json.Replace("\"Bla\"", "\"Ingles\"");
        snapshot.UpdateRecordFromJson("$.Bool1", b1);
        snapshot.UpdateRecordFromJson("$.Decimal1", d1);
        snapshot.UpdateRecordFromJson("$.DateTimeOffset1", dto1);
        snapshot.UpdateRecordFromJson("$.Model", json);
        var m2Restored = new MyModel2();
        snapshot.ExportTo(m2Restored);

        Assert.True(m2Restored.Bool1);
        Assert.Equal(127, m2Restored.Decimal1);
        Assert.Equal(DateTimeOffset.Parse(dto1.Replace("\"", "")), m2Restored.DateTimeOffset1);
        Assert.Equal("Ingles", m2Restored.Model.Name);
    }

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

    public class MyModel2 : ModelWrapper<SampleTypedModel>
    {
        public bool? Bool1 { get; set; }
        public DateTimeOffset? DateTimeOffset1 { get; set; }
        public decimal? Decimal1 { get; set; }
        public decimal Decimal2 { get; set; }
    }
}
