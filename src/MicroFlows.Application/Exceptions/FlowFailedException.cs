using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;

internal class FlowFailedException : Exception
{
    public FlowFailedException() : base()
    {
    }

    public FlowFailedException(string message) : base(message)
    {
    }
}
