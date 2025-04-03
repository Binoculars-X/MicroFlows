using BlazorForms.Rendering.MudBlazorUI.Components;
using MudBlazor;
using System.Text.Json;

namespace MicroFlows.AdminUI.Forms;

public static class GlobalSettings
{
    public const int TIMER_REFRESH_SEC = 10;
    public const int TIMER_SBUS_REFRESH_SEC = 30;

    public static readonly string DateTimeFormat = "dd/MM/yyyy HH:mm";

    //public static string? TimeZone; 

    public static EditFormOptions EditFormOptions = new EditFormOptions
    {
        MudBlazorProvidersDefined = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        TextAreaLines = 21,
        DisableDefaultButtons = true,
        InlineTableHeightPx = "340px"
    };

    public static ListFormOptions ShortListFormOptions = new ListFormOptions
    {
        MudBlazorProvidersDefined = true,
        //ShowSearch = true,
        ShowSorting = true,
        UseToolBarCaption = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        HeightPx = "400px",
        CaptionActions = "",
        FirstActionClick = false
    };

    public static ListFormOptions ListFormOptions = new ListFormOptions
    {
        MudBlazorProvidersDefined = true,
        //ShowSearch = true,
        ShowSorting = true,
        UseToolBarCaption = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        HeightPx = "470px",
        CaptionActions = "",
        FirstActionClick = false
    };

    public static ListFormOptions LargeListFormOptions = new ListFormOptions
    {
        MudBlazorProvidersDefined = true,
        //ShowSearch = true,
        ShowSorting = true,
        UseToolBarCaption = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        HeightPx = "680px",
        CaptionActions = "",
        FirstActionClick = false
    };

    public static ListFormOptions ClickableLargeListFormOptions = new ListFormOptions
    {
        MudBlazorProvidersDefined = true,
        //ShowSearch = true,
        ShowSorting = true,
        UseToolBarCaption = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        HeightPx = "640px",
        CaptionActions = "",
        FirstActionClick = true
    };

    public static ListFormOptions ClickableMediumListFormOptions = new ListFormOptions
    {
        MudBlazorProvidersDefined = true,
        //ShowSearch = true,
        ShowSorting = true,
        UseToolBarCaption = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
        HeightPx = "640px",
        CaptionActions = "",
        FirstActionClick = true
    };

    public static BoardFormOptions BoardFormOptions = new BoardFormOptions
    {
        MudBlazorProvidersDefined = true,
        Variant = Variant.Filled,
        DateFormat = "dd/MM/yyyy",
    };

    // Json
    //public static JsonSerializerOptions HumanJsonSerializerOptions = new JsonSerializerOptions
    //{
    //    Converters = { /*new JsonStringEnumConverter(),*/ new AuCustomDateTimeConverter() },
    //    PropertyNameCaseInsensitive = true,
    //    WriteIndented = true,
    //};
}

