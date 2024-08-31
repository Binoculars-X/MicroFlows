using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MicroFlows;

namespace MicroFlows.Tests.TestFlows;
public class SampleFlow : FlowBase
{
    private readonly ISampleTestStorage _storage;

    // model
    private int? _modelInt { get; set; }
    private string? _modelString { get; set; }
    private string? _id { get; set; }

    // ~ctor
    public SampleFlow(ISampleTestStorage storage)
    {
        _storage = storage;
    }

    public override async Task Execute()
    {
        await Call(async() => await Load());

        await Call(async () => await Store(_modelInt, _modelString));

        var json = await Call(async () => await Read(_id));

        var readRecord = JsonSerializer.Deserialize<(string id, int? key, string name)>(json);

        if (readRecord.id != _id || readRecord.key != _modelInt || readRecord.name != _modelString)
        {
            throw new Exception("Saved values are different with model");
        }
    }

    private async Task Load()
    {
        _modelInt = 33;
        _modelString = "test";
    }

    private async Task Store(int? key, string? name)
    {
        string id = Guid.NewGuid().ToString();
        var record = (id, key, name);
        var json = JsonSerializer.Serialize(record);
        await _storage.Save(id, json);
    }

    private async Task<string> Read(string? id)
    {
        var value = await _storage.Get(id);
        return value;
    }
}

public interface ISampleTestStorage
{
    Task<string> Save(string id, string json);
    Task<string> Get(string id);
}
