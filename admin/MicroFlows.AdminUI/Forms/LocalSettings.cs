using MicroFlows.Application.Helpers;

namespace MicroFlows.AdminUI.Forms;

public class LocalSettings
{
    public string TimeZone {  get; set; }

    public DateTime? ToLocalDateTime(DateTimeOffset? source)
    {
        if (source == null)
        {
            return null;
        }

        return TimeZoneHelper.ConvertToTimeZone(source, TimeZone)?.DateTime;
    }
}
