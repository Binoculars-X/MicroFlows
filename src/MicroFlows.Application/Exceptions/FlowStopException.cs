using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;
internal class FlowStopException : Exception
{
    public FlowStopException() : base()
    {
    }

    public FlowStopException(string message) : base(message)
    {
    }
}
