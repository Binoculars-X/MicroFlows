using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public class ModelSnapshotTests
{
    [Fact]
    public void ModelSnapshot_Should_Import_PrimitiveValues()
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

        model.ImportFrom(flow);

        var idRecord = model.Records["$.Id"];
        Assert.Equal(flow.Id, JsonSerializerEx.Deserialize(idRecord.Json, idRecord.Type));
        var modelIntRecord = model.Records["$.ModelInt"];
        Assert.Equal(flow.ModelInt, JsonSerializerEx.Deserialize(modelIntRecord.Json, modelIntRecord.Type));
        var modelDate = model.Records["$.ModelDate"];
        Assert.Equal(flow.ModelDate, JsonSerializerEx.Deserialize(modelDate.Json, modelDate.Type));
        var modelString = model.Records["$.ModelString"];
        Assert.Equal(flow.ModelString, JsonSerializerEx.Deserialize(modelString.Json, modelString.Type));
    }

    [Fact]
    public void ModelSnapshot_Should_Import_And_Export_PrimitiveValues()
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

        model.ImportFrom(flow);
        var flow2 = new SampleFlow();
        model.ExportTo(flow2);

        Assert.Equal(flow.Id, flow2.Id);
        Assert.Equal(flow.ModelInt, flow2.ModelInt);
        Assert.Equal(flow.ModelDate, flow2.ModelDate);
        Assert.Equal(flow.ModelString, flow2.ModelString);
    }
}
