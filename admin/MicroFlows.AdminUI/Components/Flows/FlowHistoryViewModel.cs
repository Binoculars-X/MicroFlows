using MicroFlows.AdminUI.Forms;
using MicroFlows.Domain.Models;

namespace MicroFlows.AdminUI.Components.Flows;

public class FlowHistoryViewModel
{
    private readonly LocalSettings _localSettings;

    public FlowStoreModel Model { get; set; } = new();

    public FlowHistoryViewModel(LocalSettings localSettings)
    {
        _localSettings = localSettings;
    }

    public List<FlowHistoryLine> GetLines()
    {
        return Model.ContextHistory.Select(c => new FlowHistoryLine(
            $"{c.CurrentTask ?? "<Start>"} at {_localSettings.ToLocalDateTime(c.CreatedOn)?.ToString(GlobalSettings.DateTimeFormat)}"))
            .ToList();
    }

}

public record FlowHistoryLine(string Header);
