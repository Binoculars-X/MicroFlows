using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;

public class FlowStoreModel
{
    /// <summary>
    /// Generated when flow executed first time
    /// </summary>
    public string RefId { get; set; } = null!;

    /// <summary>
    /// Can be provided when flow executed first time, and used later to search by it
    /// </summary>
    public string? ExternalId { get; set; } = null!;

    /// <summary>
    /// Flow type full name
    /// </summary>
    public string FlowTypeName { get; set; } = null!;

    /// <summary>
    /// List of context objects, one object per each flow step
    /// </summary>
    public List<FlowContext> ContextHistory { get; set; } = [];
}
