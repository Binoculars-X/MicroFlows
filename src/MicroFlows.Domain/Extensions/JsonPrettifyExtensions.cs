using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Extensions;

public static class JsonPrettifyExtensions
{
    public static string JsonPrettify(this string json)
    {
        using var jDoc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
        return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
    }
}
