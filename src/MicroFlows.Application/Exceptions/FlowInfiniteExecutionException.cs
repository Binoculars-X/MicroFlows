using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;

public class FlowInfiniteExecutionException : Exception
{
    public FlowInfiniteExecutionException() : base()
    {
    }

    public FlowInfiniteExecutionException(string message) : base(message)
    {
    }
}
