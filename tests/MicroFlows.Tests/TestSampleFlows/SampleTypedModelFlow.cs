using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;

public class SampleTypedModel
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? Modified { get; set; }
}

public class SampleTypedModelFlow : FlowBase<SampleTypedModel>
{
    public async Task Flow()
    {
        await CallAsync(Init);

        if (Model.Id == 33)
        {
            await CallAsync(Update);
        }

        await WaitForSignalAsync("xxx");
    }

    private async Task Init()
    {
        Model.Id = 33;
        Model.Name = "test";
    }

    private async Task Update()
    {
        Model.Modified = DateTime.UtcNow;
        Model.Id = null;
    }
}
