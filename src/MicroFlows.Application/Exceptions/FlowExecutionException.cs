using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;
internal class FlowExecutionException : Exception
{
    public FlowExecutionException() : base()
    {
    }

    public FlowExecutionException(string message) : base(message)
    {
    }
}
