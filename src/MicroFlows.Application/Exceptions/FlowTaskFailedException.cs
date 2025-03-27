using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;
internal class FlowTaskFailedException : Exception
{
    public FlowTaskFailedException() : base()
    {
    }

    public FlowTaskFailedException(string message) : base(message)
    {
    }
}
