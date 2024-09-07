﻿using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;

namespace MicroFlows;
public abstract class FlowBase : IFlow
{
    [JsonIgnore]
    public FlowParams FlowParams { get; private set; }

    //public abstract Task Execute();

    // https://stackoverflow.com/questions/1495465/get-name-of-action-func-delegate
    //public virtual async Task Call(Expression<Func<Task>> action)

    public virtual void Call(Action action)
    {
        action();
    }

    public virtual async Task CallAsync(Func<Task> action)
    {
        await action();
    }

    public virtual async Task<T> CallAsync<T>(Func<Task<T>> action)
    {
        return await action();
    }

    public virtual void WaitForCondition(Func<bool> action)
    {
        if (action())
        {
            return;
        }

        throw new FlowStopException();
    }

    // ToDo:
    // void Call()
    // T Call<T>()
    // ExecuteActivityAsync()
    // WaitForConditionAsync()
    // Signals

    public void SetModel(ModelSnapshot source)
    {
        source.ExportTo(this);
    }

    public void SetParams(FlowParams flowParams)
    {
        FlowParams = flowParams;
    }
}