using JsonPathToModel.Helpers;
using MicroFlows.AdminUI.Forms;
using MicroFlows.AdminUI.Models;
using MicroFlows.Application.Services;
using MicroFlows.Domain.Models;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroFlows.AdminUI.Components.Flows;

public class FlowHistoryItemViewModel
{
    private readonly LocalSettings _localSettings;
    private readonly IFlowAdminProvider _flowAdminProvider;

    public FlowContext Model { get; set; }
    public int Index { get; set; }
    public FlowHistoryViewModel Parent { get; set; }

    public FlowHistoryItemViewModel(LocalSettings localSettings, 
        IFlowAdminProvider flowAdminProvider)
    {
        _localSettings = localSettings;
        _flowAdminProvider = flowAdminProvider;
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
        await _flowAdminProvider.DeleteExecutionSteps(Model.RefId, Index);
        await Parent.ReloadModel(Model.RefId);
    }

    public async Task UpdateModel(List<EditModelDetails> data)
    {
        foreach (var item in data.Where(i => i.Changed))
        {
            Model.Model.UpdateRecordFromJson(item.Key, item.Value);
            //Model.Model.Records[item.Key] = Model.Model.Records[item.Key].FromJson(item.Value); 
                //with { Json = JsonSerializer.Serialize(item.Value) };
        }

        await _flowAdminProvider.UpdateFlowContextModel(Model.RefId, Index, Model.Model);
        await Parent.ReloadModel(Model.RefId);
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
