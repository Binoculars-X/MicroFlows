using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleFlow : FlowBase
{
    // model consists of all public serializable properties
    public DateTime? ModelDate { get; set; }
    public int? ModelInt { get; set; }
    public string? ModelString { get; set; }
    public string? Id { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);

        if (ModelInt == 33 && ModelString == "test")
        {

        }

        await CallAsync(async () => await Update(ModelInt, ModelString));

        if (Id != null)
        {

        }

        Call(() => 
        { 
            if (Params["flag"] == "stop")
            {
                throw new FlowStopException();
            }
        });
    }

    public virtual async Task MyMethod()
    {
        ModelDate = new DateTime(1912,12,31);
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
        Id = id;
        //var json = JsonSerializer.Serialize(record);
        //await _storage.Save(id, json);
    }

    private async Task<string> Read(string? id)
    {
        var value = id;
        return value;
    }
}
