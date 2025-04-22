using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorForms.Rendering.MudBlazorUI.Components;

public class EditFormOptions : FormOptions
{
    public bool DisableDefaultButtons { get; set; }

    public string InlineTableHeightPx { get; set; } = "360px";
}
