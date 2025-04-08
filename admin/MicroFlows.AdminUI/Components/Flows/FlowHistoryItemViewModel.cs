using MicroFlows.AdminUI.Forms;
using MicroFlows.Domain.Models;

namespace MicroFlows.AdminUI.Components.Flows;

public class FlowHistoryItemViewModel
{
    private readonly LocalSettings _localSettings;

    public FlowContext Model { get; set; }
    public int Index { get; set; }
    public FlowHistoryViewModel Parent { get; set; }

    public FlowHistoryItemViewModel(LocalSettings localSettings)
    {
        _localSettings = localSettings;
    }

    public string GetDeleteMessage()
    {
        var num = Parent.Model.ContextHistory.Count - Index;
        return $"Are you sure you want to delete {num} step(s) from the execution history?";
    }

    public async Task DeleteSteps()
    { 
    }

    public string GetModel()
    {
        return Model.Model.ToJson();
    }

    public string GetException()
    {
        return $@"{Model.ExecutionResult.ExceptionMessage}
Stack Trace:
{Model.ExecutionResult.ExceptionStackTrace}
";
    }
}
