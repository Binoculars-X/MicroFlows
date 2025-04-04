using JsonPathToModel;
using MicroFlows.AdminUI.Forms;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroFlows.AdminUI.Components.Flows;

public class FlowHistoryViewModel
{
    private static LocalSettings __localSettings;
    private readonly LocalSettings _localSettings;

    public FlowStoreModel Model { get; set; } = new();

    public FlowHistoryViewModel(LocalSettings localSettings)
    {
        _localSettings = localSettings;
        __localSettings = localSettings;
    }

    public List<FlowHistoryLine> GetLines()
    {
        return Model.ContextHistory.Select(c => new FlowHistoryLine(
            $"{_localSettings.ToLocalDateTime(c.CreatedOn)
                ?.ToString(GlobalSettings.DateTimeFormat)} {c.CurrentTask ?? "<Start>"}",
            c
            )).ToList();
    }

    public string? GetFlowParameters()
    {
        var ps = Model.ContextHistory.FirstOrDefault()?.Params;

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
        var payload = Model.ContextHistory.FirstOrDefault()?.Params?.Payload;

        if (payload != null)
        {
            var modelSnapshot = JsonSerializer.Deserialize<ModelSnapshot>(payload);
            return modelSnapshot.ToJson();
        }

        return null;
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

public class FlowHistoryItemViewModel
{
    private readonly LocalSettings _localSettings;

    public FlowContext Model { get; set; } = new();

    public FlowHistoryItemViewModel(LocalSettings localSettings)
    {
        _localSettings = localSettings;
    }

    public string GetModel()
    {
        return Model.Model.ToJson();
    }
}
