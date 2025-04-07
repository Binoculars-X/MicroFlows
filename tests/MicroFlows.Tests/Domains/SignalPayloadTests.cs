using JsonPathToModel;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Domains;

public class SignalPayloadTests
{
    [Fact]
    public void SnapshotRecord_Deserializes_From_Pure_Json()
    {
        var model = new MyModel { Id = 3, Column1 = "data" };
        var record = new SnapshotRecord(SignalPayload.JsonType, JsonSerializer.Serialize(model));

        var restoredModel = record.Deserialize<MyModel>();

        Assert.Equal(model.Id, restoredModel.Id);
        Assert.Equal(model.Column1, restoredModel.Column1);

        var recordJson = JsonSerializer.Serialize(record);
        var restoredRecord = JsonSerializer.Deserialize<SnapshotRecord>(recordJson);

        var restoredRestoredModel = restoredRecord.Deserialize<MyModel>();

        Assert.Equal(model.Id, restoredRestoredModel.Id);
        Assert.Equal(model.Column1, restoredRestoredModel.Column1);
    }

    [Fact]
    public void ModelSnapshot_Deserializes_Model_From_Pure_Json()
    {
        var model = new MyModel { Id = 3, Column1 = "data" };
        var wrapped = new ModelWrapper<MyModel> { Model = model };
        var snapshot = new ModelSnapshot();
        snapshot.ImportFrom(wrapped);

        var newJson = "{\"Id\":771,\"Column1\":\"new data\"}";
        snapshot.Records["$.Model"] = snapshot.Records["$.Model"] with { Json = newJson };

        var restoredModel = new ModelWrapper<MyModel>();
        snapshot.ExportTo(restoredModel);

        Assert.Equal(771, restoredModel.Model.Id);
        Assert.Equal("new data", restoredModel.Model.Column1);
    }

    [Fact]
    public void SignalPayload_Deserializes_From_Pure_Json()
    {
        var model = new MyModel { Id = 337, Column1 = "some data" };
        var record = new SnapshotRecord(SignalPayload.JsonType, JsonSerializer.Serialize(model));
        var payload = new SignalPayload { Record = record };

        var restoredModel = payload.GetValue<MyModel>();

        Assert.Equal(model.Id, restoredModel.Id);
        Assert.Equal(model.Column1, restoredModel.Column1);
    }

    public class MyModel
    {
        public int Id { get; set; }
        public string? Column1 { get; set; }
    }
}
