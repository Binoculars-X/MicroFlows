using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;
internal class FlowValidationException : Exception
{
    public FlowValidationException() : base()
    {
    }

    public FlowValidationException(string message) : base(message)
    {
    }
}
