using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Versioning;

public class FlowVersionTests
{
    [Fact]
    public void FlowVersionSelector_Should_UseNamingConvention()
    {
        Assert.Equal("V1", new SmapleFlowV1().GetVersion());
        Assert.Equal("V2", new SmapleFlowV2().GetVersion());
        Assert.Equal("3", new SmapleFlow_3().GetVersion());
        Assert.Equal("4", new SmapleFlow4().GetVersion());
        Assert.Equal("3", new SmapleFlowOld3().GetVersion());
        Assert.Equal("V1", new SmapleFlowOld().GetVersion());
    }

    [Fact]
    public void FlowVersionSelector_Should_AllowReadingVersions()
    { }
}

public class SmapleFlow : FlowVersionSelector
{
    public override void RegisterFlowVersion(List<Type> versions)
    {
        versions.Add(typeof(SmapleFlowV1));
        versions.Add(typeof(SmapleFlowV2));
        versions.Add(typeof(SmapleFlow_3));
        versions.Add(typeof(SmapleFlow4));
    }
}

public class SmapleFlowV1 : FlowBase
{ }

public class SmapleFlowV2 : FlowBase
{ }

public class SmapleFlow_3 : FlowBase
{ }
public class SmapleFlow4 : FlowBase
{ }

public class SmapleFlowOld3 : FlowBase
{ }

public class SmapleFlowOld : FlowBase
{ }
