using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MicroFlows.Domain.Enums;

public enum FlowStateEnum
{
    Stop = 0,
    Start,
    Continue,
    Finished,
    Failed,
    Waiting
}

public enum ResultStateEnum
{
    Fail = 0,
    Success
}
