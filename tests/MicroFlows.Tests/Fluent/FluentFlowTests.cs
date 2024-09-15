using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.Tests.TestSampleFlows.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Fluent;

public class FluentFlowTests
{
    [Fact]
    public void FluentFlow_Should_Be_Detected_WhenDefineOverriden()
    {
        Assert.Equal(typeof(SampleFluentFlow), typeof(SampleFluentFlow).GetMethod("Define").DeclaringType);
    }

    [Fact]
    public void FluentFlow_ShouldNot_Be_Detected_WhenDefineNotOverriden()
    {
        Assert.Equal(typeof(FlowBase), typeof(SampleFlow).GetMethod("Define").DeclaringType);
    }
}
