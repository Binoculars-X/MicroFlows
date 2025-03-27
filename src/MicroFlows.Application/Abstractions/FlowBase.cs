using MicroFlows.Application.Helpers;
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
using MicroFlows.Application;
using Microsoft.Extensions.DependencyInjection;

namespace MicroFlows;

/// <summary>
/// Base class for typed model flows
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
public abstract partial class FlowBase<TModel> : FlowBase where TModel : class, new()
{
    public TModel Model { get; set; } = new();
}

/// <summary>
/// Base class for Flows
/// All properties and fields except privates will be tracked as Model
/// </summary>
public abstract partial class FlowBase : IFlow
{
    [JsonIgnore]
    public string RefId { get; set; }
    //[JsonIgnore]
    //public string ExternalId { get; set; }

    [JsonIgnore]
    public FlowParams Params { get; private set; } = new();

    [JsonIgnore]
    protected Dictionary<string, Func<SignalPayload, Task>> _signalHandlers = [];

    /// <summary>
    /// it should not be stored in ModelSnapshot, it is stored in FlowStoreModel level
    /// </summary>
    [JsonIgnore]
    public List<SignalJournalEntry> SignalJournal { get; internal set; } = [];

    /// <summary>
    /// it should not be stored in ModelSnapshot, it is stored in FlowStoreModel level
    /// </summary>
    [JsonIgnore]
    public List<string> Patches { get; internal set; } = [];

    // https://stackoverflow.com/questions/1495465/get-name-of-action-func-delegate
    //public virtual async Task Call(Expression<Func<Task>> action)

    [JsonIgnore]
    private IFlowProvider _flowProvider = null!;

    /// <summary>
    /// returns version number using naming convention SampleFlowV2 => V2
    /// </summary>
    /// <returns></returns>
    public virtual string GetVersion() 
    { 
        var name = GetType().Name;
        var version = "";

        for  (int i = name.Length-1; i >= 0; i--)
        {
            if (char.IsDigit(name[i]))
            {
                version = name[i] + version;
            }
            else if (name[i].ToString().ToLower() == "v")
            {
                version = name[i] + version;
                break;
            }
            else
            {
                break;
            }
        }

        if (version == name || version == "")
        {
            return "V1";
        }

        return version;
    }

    /// <summary>
    /// Flows add patches here, the patches saved to the storage when flow instance executed first time
    /// </summary>
    /// <param name="patch"></param>
    public void RegisterPatch(string patch)
    {
        Patches.Add(patch);
    }

    /// <summary>
    /// returns if current floe instance (stored in the database) has a particular path
    /// </summary>
    /// <param name="patch"></param>
    /// <returns></returns>
    public bool IsPatch(string patch)
    {
        return Patches.Contains(patch); 
    }


    /// <summary>
    /// Should be overriden to add all registered patches
    /// Executed by FlowEngine
    /// </summary>
    /// <param name="patches"></param>
    public virtual void RegisterPatches(List<string> patches)
    {
    }

    /// <summary>
    /// Should be supplied by FlowEngine
    /// Is used to generate required dependencies for example to IFlowProvider
    /// </summary>
    /// <param name="services"></param>
    internal void SetServiceProvider(IServiceProvider services)
    {
        _flowProvider = services.GetService<IFlowProvider>()!;
    }

    /// <summary>
    /// Execute another flow in a separate thread
    /// </summary>
    /// <param name="ps"></param>
    /// <returns></returns>
    public async Task ExecuteFlow(FlowParams ps)
    {
        await Task.Run(() =>
        {
            _flowProvider.ExecuteFlow(ps);
        });
    }

    /// <summary>
    /// Send Signal to another flow in a separate thread
    /// </summary>
    /// <param name="ps"></param>
    /// <param name="signal"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public async Task SendSignal(FlowParams ps, string signal, object? payload = null)
    {
        await Task.Run(() =>
        {
            _flowProvider.SendSignal(ps, signal, payload);
        });
    }

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
        Params = flowParams;
    }
}
