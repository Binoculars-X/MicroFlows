using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public abstract class FlowBase : IFlow
{
    public abstract Task Execute();

    public virtual async Task Call(Func<Task> action)
    { }

    public virtual async Task<T> Call<T>(Func<Task<T>> action)
    {
        return await action();
    }

    public virtual void SetModel(IFlow source)
    { }
    public virtual void SetParams(FlowParams flowParams)
    { }
}
