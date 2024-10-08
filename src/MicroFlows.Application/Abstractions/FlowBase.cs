﻿using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;
using JsonPathToModel;
using System.Linq;

namespace MicroFlows;
public abstract class FlowBase : IFlow
{
    [JsonIgnore]
    public string RefId { get; set; }

    [JsonIgnore]
    public FlowParams FlowParams { get; private set; }

    [JsonIgnore]
    protected Dictionary<string, Func<SignalPayload, Task>> _signalHandlers = [];

    // it should not be stored in ModelSnapshot, it is stored in FlowStoreModel level
    [JsonIgnore]
    public List<SignalJournalEntry> SignalJournal { get; internal set; } = [];

    // https://stackoverflow.com/questions/1495465/get-name-of-action-func-delegate
    //public virtual async Task Call(Expression<Func<Task>> action)

    /// <summary>
    /// Saves SignalHandler delegate for trigerring when a signal comes
    /// </summary>
    /// <param name="signal"></param>
    /// <param name="handler"></param>
    public virtual void AddSignalHandler(string signal, Func<SignalPayload, Task> handler)
    {
        _signalHandlers[signal] = handler;
    }

    public virtual void Call(Action action)
    {
        action();
    }

    public virtual async Task CallAsync(Func<Task> action)
    {
        await action();
    }

    // ToDo: will not work until InterceptAsynchronous<TResult> is not implemented
    //public virtual async Task<T> CallAsync<T>(Func<Task<T>> action)
    //{
    //    return await action();
    //}

    /// <summary>
    /// Checks if signal received without blocking the flow execution
    /// If signal received then registered signal handler will be triggered to read the payload
    /// </summary>
    /// <param name="signalName"></param>
    /// <returns></returns>
    public virtual async Task CheckSignalReceivedAsync(string signalName)
    {
        var entry = SignalJournal.LastOrDefault(r => r.Signal == signalName);

        if (entry != null)
        {
            if (_signalHandlers.ContainsKey(signalName))
            {
                var payload = new SignalPayload() { Value = entry.Record?.Deserialize() };
                await _signalHandlers[signalName](payload);
            }

            return;
        }
    }

    /// <summary>
    /// Stops flow until signal with signalName received
    /// If signal received then registered signal handler will be triggered to read the payload
    /// </summary>
    /// <param name="signalName"></param>
    /// <returns></returns>
    /// <exception cref="FlowStopException"></exception>
    public virtual async Task WaitForSignalAsync(string signalName)
    {
        var entry = SignalJournal.LastOrDefault(r => r.Signal == signalName);
        
        if (entry != null)
        {
            if (_signalHandlers.ContainsKey(signalName))
            {
                var payload = new SignalPayload() { Value = entry.Record?.Deserialize() };
                await _signalHandlers[signalName](payload);
            }

            return;
        }

        throw new FlowStopException("WaitForSignal");
    }

    public virtual void WaitForCondition(Func<bool> action)
    {
        if (action())
        {
            return;
        }

        throw new FlowStopException("WaitForCondition");
    }

    // ToDo:
    // void Call()
    // T Call<T>()
    // ExecuteActivityAsync()
    // WaitForConditionAsync()

    public void SetModel(ModelSnapshot source)
    {
        source.ExportTo(this);
    }

    public void SetParams(FlowParams flowParams)
    {
        FlowParams = flowParams;
    }
}
