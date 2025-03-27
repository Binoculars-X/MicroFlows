using MicroFlows.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.TestSampleFlows;

public class SampleFlow : FlowBase
{
    public bool InitPassed { get; set; }
    public bool UpdatePassed { get; set; }
    public bool InlinePassed { get; set; }
    public bool CallInlinePassed { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);
        await CallAsync(async () => await Update(1, "String"));
        
        InlinePassed = true;

        Call(() => CallInlinePassed = true);
    }

    private async Task Init()
    {
        InitPassed = true;
    }

    private async Task Update(int? key, string? name)
    {
        UpdatePassed = true;
    }
}
