using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleWithExceptionInBodyFlow : FlowBase
{
    // !!! this is only for testing, don't do that in real flows
    private static bool _blink = true;

    protected int? ModelInt { get; set; }
    protected string? Id { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);

        await CallAsync(Update);

        if (_blink)
        {
            _blink = false;
            throw new Exception("Expected null Id here");
        }

        Id = "recovered";

        WaitForCondition(() => false);
    }

    private async Task Init()
    {
        Id = "id:test";
    }

    private async Task Update()
    {
        ModelInt = 33;
    }
}
