using MicroFlows.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public class TimeZoneHelperTests
{
    [Fact]
    public void TimeZoneHelper_Converts_Utc_To_Sydney()
    {
        var timeZone = "Australia/Sydney";
        var dateTime = DateTime.Now;
        var utc = DateTimeOffset.UtcNow;
        var local = TimeZoneHelper.ConvertToTimeZone(utc, timeZone);
        var converted = local.Value.DateTime;

        Assert.Equal(dateTime.Date, converted.Date);
        Assert.Equal(dateTime.Day, converted.Day);
        Assert.Equal(dateTime.Hour, converted.Hour);
        Assert.Equal(dateTime.Minute, converted.Minute);
    }

    [Fact]
    public void TimeZoneHelper_Converts_Sydney_To_Utc()
    {
        var utcTimezone = "Etc/UTC";
        var timeZone = "Australia/Sydney";
        var dateTime = DateTime.Now;
        var utc = DateTimeOffset.UtcNow;
        var local = TimeZoneHelper.ConvertDateTimeToDateTimeOffset(dateTime, timeZone);
        var utcConverted = TimeZoneHelper.ConvertToTimeZone(local, utcTimezone);

        Assert.Equal(utc.Date, utcConverted.Value.Date);
        Assert.Equal(utc.Day, utcConverted.Value.Day);
        Assert.Equal(utc.Hour, utcConverted.Value.Hour);
        Assert.Equal(utc.Minute, utcConverted.Value.Minute);
    }

    [Fact]
    public void TimeZoneHelper_ConvertDateTimeToUtc_Returns_Utc()
    {
        var timeZone = "Australia/Sydney";
        var dateTime = DateTime.Now;
        var utc = DateTimeOffset.UtcNow;
        var converted = TimeZoneHelper.ConvertDateTimeToUtc(dateTime, timeZone);

        Assert.Equal(utc.Date, converted.Value.Date);
        Assert.Equal(utc.Day, converted.Value.Day);
        Assert.Equal(utc.Hour, converted.Value.Hour);
        Assert.Equal(utc.Minute, converted.Value.Minute);
    }
}
