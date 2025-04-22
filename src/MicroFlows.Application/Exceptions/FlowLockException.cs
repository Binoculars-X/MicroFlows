using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;

public class FlowLockException : Exception
{
    public FlowLockException() : base()
    {
    }

    public FlowLockException(string message) : base(message)
    {
    }
}
