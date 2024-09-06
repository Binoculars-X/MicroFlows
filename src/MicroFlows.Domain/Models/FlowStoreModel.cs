using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;
public class FlowStoreModel
{
    public string FlowTypeName { get; set; } = null!;
    public List<FlowContext> ContextHistory { get; set; } = [];
}
