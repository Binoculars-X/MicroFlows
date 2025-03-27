using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Versioning;

public class FlowPatchesTests
{
    [Fact]
    public void FlowPatches_Should_BeReturned_IfRegistered()
    {
        var flow = new SmapleFlowWithPatches();
    }
}

public class SmapleFlowWithPatches : FlowBase
{
    public const string Patch313 = "Patch313";
    public const string Patch317 = "Patch317";

    public bool Patch313Status;

    public async Task Flow()
    {
        if (IsPatch(Patch313))
        {
            // only instances that created after this patch applied come to here without breaking determinism
            // if flow instance created before this path applied, it will not come here
            Patch313Status = true;
        }
    }

    public override void RegisterPatches(List<string> patches)
    {
        patches.Add(Patch313);
        patches.Add(Patch317);
    }
}