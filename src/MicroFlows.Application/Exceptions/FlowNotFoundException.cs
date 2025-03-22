using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;

public class FlowNotFoundException : Exception
{
    public FlowNotFoundException() : base()
    {
    }

    public FlowNotFoundException(string message) : base(message)
    {
    }
}
