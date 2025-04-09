using MicroFlows.AdminUI.Forms;
using MicroFlows.AdminUI.Models;
using MicroFlows.Domain.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public List<EditModelDetails> GetModelDetails()
    {
        var details = Model.Model.Records.Select(r => new EditModelDetails 
        { 
            Key = r.Key,
            Name = r.Key.Split('.').Last(),
            Type = r.Value.Type,
            Value = r.Key == "$.Model"? r.Value.Json.JsonPrettify() : r.Value.Json
        }).ToList();

        return details;
    }

    public string GetDeleteMessage()
    {
        var num = Parent.Model.ContextHistory.Count - Index;
        return $"Are you sure you want to delete the last {num} step(s) from the execution history?";
    }

    public async Task DeleteSteps()
    { 
    }

    public async Task UpdateModel(List<EditModelDetails> data)
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
