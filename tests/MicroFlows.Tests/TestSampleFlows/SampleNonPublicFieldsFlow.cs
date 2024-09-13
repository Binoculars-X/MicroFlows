using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Tests.TestSampleFlows;
public class SampleNonPublicFieldsFlow : FlowBase
{
    protected internal DateTime? _modelDate;
    internal int? _modelInt;
    protected string? _modelString;
    protected string? _modelStringProperty { get; set; }

    public DateTime? AccessorModelDate { get { return _modelDate; } }
    public int? AccessorModelInt { get { return _modelInt; } }
    public string? AccessorModelString { get { return _modelString; } }

    public async Task Flow()
    {
        await CallAsync(Init);

        if (_modelInt == 33 && _modelString == "test")
        {

        }

        await CallAsync(async () => await Update(_modelInt, _modelString));

        if (_modelInt != null)
        {

        }

        Call(() =>
        {
            if (_modelInt == null)
            {
                throw new FlowStopException();
            }
        });
    }

    private async Task Init()
    {
        _modelInt = 33;
        _modelString = "test";
    }

    private async Task Update(int? key, string? name)
    {
        _modelDate = DateTime.UtcNow;
        _modelInt = null;
    }
}
