using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public class FlowOptions
{
    public bool NoStorage { get; set; }
    public FlowExecutionStoreModel StoreModel { get; set; }
}

public enum FlowExecutionStoreModel
{
    Full = 1,
    FullNoHistory,
    NoStoreTillStop,
}
