using BlazorForms.Forms.Definitions.FluentForms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorForms.Forms;

public class ConfirmationDetailsExtended : ConfirmationDetails
{
	public string? MessageBinding { get; set; }
	public string? DialogType { get; set; }
	public string? Anchor { get; set; }
}
