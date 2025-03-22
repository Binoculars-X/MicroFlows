using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Infrastructure.Tests.TestSampleFlows.Fluent;

public class LinearModel
{
    public bool FlowStartExecuted { get; set; }
    public bool NextExecuted { get; set; }
    public bool Next2Executed { get; set; }
    public bool FlowEndExecuted { get; set; }
    public bool Condition1 { get; set; }
}

public class LinearInlineFlow : FlowBase<LinearModel>
{
    public override void Define(IFlowBuilder builder)
    {
        builder
            .Begin(() => { Model.FlowStartExecuted = true; })
            .Next(() => { Model.NextExecuted = true; })
            .Next(() => { Model.Next2Executed = true; })
            .End(() => { Model.FlowEndExecuted = true; });
    }
}
