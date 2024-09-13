using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicroFlows;

namespace MicroFlows.Tests.TestFlows;

public class SampleStoringFlow : FlowBase
{
    //[JsonIgnore]
    private readonly ISampleTestStorage _storage;

    // model consists of all public serializable properties
    public int? ModelInt { get; set; }
    public string? ModelString { get; set; }
    public string? Id { get; set; }

    // ~ctor
    public SampleStoringFlow(ISampleTestStorage storage)
    {
        _storage = storage;
    }

    public async Task Flow()
    { }

    private async Task Load()
    {
        ModelInt = 33;
        ModelString = "test";
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
