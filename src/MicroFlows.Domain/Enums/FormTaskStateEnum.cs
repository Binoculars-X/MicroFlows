using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Enums;

public enum FormTaskStateEnum
{
    Initialized = 0,
    Loaded,
    Saved,
    Submitted,
    Rejected
}
