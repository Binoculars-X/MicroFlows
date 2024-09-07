using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Application.Exceptions;

public class NonDeterministicFlowException : Exception
{
    public NonDeterministicFlowException() : base()
    {
    }

    public NonDeterministicFlowException(string message) : base(message)
    {
    }
}
