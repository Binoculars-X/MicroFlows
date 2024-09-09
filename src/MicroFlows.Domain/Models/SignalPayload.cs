using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public class SignalPayload
{
    public object? Value { get; internal set; }
    
    public T? GetValue<T>()
    {
        return (T?)Value;
    }
}
