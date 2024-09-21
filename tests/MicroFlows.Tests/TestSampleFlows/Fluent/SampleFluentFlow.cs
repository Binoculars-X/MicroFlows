using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.TestSampleFlows.Fluent;

public class SampleFluentFlow : FlowBase
{
    public override void Define(IFlowBuilder builder)
    {
        builder
            .Begin(() => FlowStart())
            .Next(() => LoadData())
            //.NextForm(typeof(TestSampleForm1))
            .If(() => 1 == 1)
                .Label("label1")
                .Next(() => { var someStatement = "1"; })
            .Else()
                .Next(() => { var someOtherStatement = "2"; })
            //.Goto("label1")
            .EndIf()
            .Next(() => RefreshFormData())
            //.NextForm(typeof(TestSampleForm2))
            .Next(() => SaveData())
            .End(() => FlowEnd());
    }

    public async Task FlowStart()
    {
    }

    public async Task LoadData()
    {
    }

    public async Task RefreshFormData()
    {
    }
    public async Task SaveData()
    {
    }

    public async Task FlowEnd()
    {
    }
}
