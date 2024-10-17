using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleWithNonReadonlyDependenciesFlow : FlowBase
{
    protected IFlowProvider _flowProvider;

    protected internal DateTime? _modelDate;
    internal int? _modelInt;
    protected string? _modelString;
    protected string? _modelStringProperty { get; set; }

    public SampleWithNonReadonlyDependenciesFlow(IFlowProvider flowProvider)
    { 
        _flowProvider = flowProvider;
    }

    public async Task Flow()
    {
        await CallAsync(Init);

        if (_modelInt == 33)
        {
            await CallAsync(Update);
        }

        await WaitForSignalAsync("xxx");
    }

    private async Task Init()
    {
        _modelInt = 33;
        _modelString = "test";
        
        if (_flowProvider == null)
        { 
        }
    }

    private async Task Update()
    {
        _modelDate = DateTime.UtcNow;
        _modelInt = null;
    }
}
