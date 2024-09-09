using MicroFlows.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public class JsonSerializerExTests
{
    [Fact]
    public void JsonSerializerEx_Should_DeserializeFromKnownType()
    {
        var model = new Model()
        {
            Id = 7,
            Name = "Text",
            Records = { { "One", new ModelRecord() { Id = 12, Name = "Nested Text" } } }
        };

        var json = JsonSerializer.Serialize(model);

        var restored = JsonSerializerEx.Deserialize(json, model.GetType().FullName!) as Model;

        Assert.Equal(model.Id, restored!.Id);
        Assert.Equal(model.Name, restored.Name);
        Assert.Equal(model.Records["One"].Id, restored.Records["One"].Id);
        Assert.Equal(model.Records["One"].Name, restored.Records["One"].Name);
    }
}

public class Model
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public Dictionary<string, ModelRecord> Records { get; set; } = [];
}

public class ModelRecord
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

