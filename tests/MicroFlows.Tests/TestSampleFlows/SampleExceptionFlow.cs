using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleExceptionFlow : FlowBase
{
    // model consists of all public serializable properties
    public DateTime? ModelDate { get; set; }
    public int? ModelInt { get; set; }
    public string? ModelString { get; set; }
    public string? Id { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);

        await CallAsync(async () => await Update(ModelInt, ModelString));

        throw new Exception("Some crazy error happened");
    }

    private async Task Init()
    {
        ModelInt = 33;
        ModelString = "test";
    }

    private async Task Update(int? key, string? name)
    {
        ModelString = ModelString + name;
    }
}
