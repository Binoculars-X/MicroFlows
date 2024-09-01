using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleFlow : FlowBase
{
    // model consists of all public serializable properties
    public int? ModelInt { get; set; }
    public string? ModelString { get; set; }

    // not model?
    private string? _id { get; set; }

    public override async Task Execute()
    {
        await Call(async () => await Init());

        await Call(async () => await Update(ModelInt, ModelString));

        var json = await Call(async () => await Read(_id));
    }

    private async Task Init()
    {
        ModelInt = 33;
        ModelString = "test";
    }

    private async Task Update(int? key, string? name)
    {
        string id = Guid.NewGuid().ToString();
        var record = (id, key, name);
        //var json = JsonSerializer.Serialize(record);
        //await _storage.Save(id, json);
    }

    private async Task<string> Read(string? id)
    {
        var value = id;
        return value;
    }
}
