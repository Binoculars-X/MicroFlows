using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleWithExceptionInDelegateFlow : FlowBase
{
    // !!! this is only for testing, don't do that in real flows
    private static bool _blink = true;

    protected int? ModelInt { get; set; }
    protected string? Id { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);

        await CallAsync(Update);

        await CallAsync(CheckStatus);

        // we should save the final context with final model
        Id = "recovered";
        ModelInt = 414;
    }

    private async Task Init()
    {
        Id = "id:test";
    }

    private async Task Update()
    {
        ModelInt = 33;
    }

    private async Task CheckStatus()
    {
        if (_blink)
        {
            _blink = false;
            throw new Exception("Some error code here");
        }
    }
}
