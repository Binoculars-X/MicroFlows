using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public class StringExtensionsTests
{
    [Fact]
    public void CompactMiddle_Returns_Compacted_Values()
    {
        Assert.Equal(null, ((string)null).CompactMiddle(6));
        Assert.Equal("Ab..:1", "Abcdfg:1".CompactMiddle(6));
        Assert.Equal("WaitForSignalTimeou..ervationConfirmed:5", "WaitForSignalTimeoutAsync_ReservationConfirmed:5".CompactMiddle(40));
        Assert.Equal("Abcdfg:1", "Abcdfg:1".CompactMiddle(40));
    }
}
