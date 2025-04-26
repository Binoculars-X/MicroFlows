using JsonPathToModel;
using MicroFlows.AdminUI.Forms;
using MicroFlows.AdminUI.Models;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroFlows.AdminUI.Components.Flows;

public class FlowHistoryViewModel
{
    private static LocalSettings __localSettings;
    private readonly LocalSettings _localSettings;
    private readonly IFlowRepository _flowRepository;
    private readonly IFlowAdminProvider _flowAdminProvider;

    public FlowStoreModel? Model { get; set; } = null;

    public FlowHistoryViewModel(LocalSettings localSettings, IFlowRepository flowRepository,
        IFlowAdminProvider flowAdminProvider)
    {
        _localSettings = localSettings;
        __localSettings = localSettings;
        _flowRepository = flowRepository;
        _flowAdminProvider = flowAdminProvider;
    }

    public async Task ReloadModel(string? refId)
    {
        if (refId == null)
        {
            Model = null;
        }
        else
        {
            Model = await _flowRepository.GetFlowModel(refId);
        }
    }

    public async Task RestartFlow()
    {
        //await _flowAdminProvider.UpdateFlowStatus(Model.RefId, FlowStateEnum.Rerun);
        await _flowAdminProvider.RerunFlow(Model.RefId);
        await ReloadModel(Model.RefId);
    }

    public async Task StopFlow()
    {
        //await _flowAdminProvider.UpdateFlowStatus(Model.RefId, FlowStateEnum.Halt);
        await _flowAdminProvider.HaltFlow(Model.RefId);
        await ReloadModel(Model.RefId);
    }

    public List<FlowHistoryLine> GetLines()
    {
        return Model?.ContextHistory.Select(c => new FlowHistoryLine(
            $"{_localSettings.ToLocalDateTime(c.CreatedOn)
                ?.ToString(GlobalSettings.DateTimeFormat)} {c.CurrentTask.CompactMiddle(40) ?? "<Unknown>"}",
            c
            )).ToList();
    }

    public string? GetFlowParameters()
    {
        var ps = Model?.Params;

        if (ps == null)
        {
            return null;
        }

        if (ps?.Payload != null)
        {
            var copy = TypeHelper.CloneObject(ps);
            copy.Payload = "_json_";
            ps = copy;
        }

        return JsonSerializer.Serialize(ps, new JsonSerializerOptions { WriteIndented = true });
    }

    public string? GetFlowParametersPayload()
    {
        var payload = Model?.Params?.Payload;

        if (payload != null)
        {
            var modelSnapshot = JsonSerializer.Deserialize<ModelSnapshot>(payload);
            return modelSnapshot.ToJson();
        }

        return null;
    }

    public bool RunDisabled 
    {
        get
        {
            return Model.State != FlowStateEnum.Finished && Model.State != FlowStateEnum.Failed 
                && Model.State != FlowStateEnum.Stop && Model.State != FlowStateEnum.Halt;
        }
    }

    public bool StopDisabled
    {
        get
        {
            return Model.State != FlowStateEnum.Waiting
                && Model.State != FlowStateEnum.Start && Model.State != FlowStateEnum.Continue;
        }
    }

    public string? GetSignals()
    {
        if (Model.SignalJournal.Any())
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            options.Converters.Add(new CustomDateTimeOffsetConverter());

            return JsonSerializer.Serialize(Model.SignalJournal, options);
        }

        return null;
    }

    public async Task SaveSignalChanges(List<SignalJournalEntryDetails> details)
    {
        var newList = details.Where(x => x.Deleted == false).Select(x => x.Entry).ToList();
        await _flowAdminProvider.UpdateFlowSignals(Model.RefId, newList);
        await ReloadModel(Model.RefId);
    }

    public class CustomDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            var val = __localSettings.ToLocalDateTime(value)?.ToString(GlobalSettings.DateTimeFormat);
            writer.WriteStringValue(val);
        }
    }
}

public record FlowHistoryLine(string Header, FlowContext Context);

