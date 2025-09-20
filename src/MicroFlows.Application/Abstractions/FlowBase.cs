using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Abstractions;
using JsonPathToModel;
using System.Linq;
using MicroFlows.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using Castle.Components.DictionaryAdapter.Xml;

namespace MicroFlows;

/// <summary>
/// Base class for typed model flows
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
public abstract partial class FlowBase<TModel> : FlowBase where TModel : class, new()
{
    public TModel Model { get; set; } = new();

    public new void LoadModelFromParams()
    {
        if (Params?.Payload == null)
        {
            throw new FlowExecutionException("Params.Payload cannot be null");
        }

        var modelSnapshot = JsonSerializer.Deserialize<ModelSnapshot>(Params.Payload);
        modelSnapshot.ExportTo(Model);
    }
}

/// <summary>
/// Base class for Flows
/// All properties and fields except privates will be tracked as Model
/// </summary>
public abstract partial class FlowBase : IFlow
{
    public const string TIMEOUT_HANDLER = "TIMEOUT_HANDLER";
    //public static string Name() 
    //{
    //    var method = MethodBase.GetCurrentMethod();
    //    return method.DeclaringType.FullName; 
    //}

    [JsonIgnore]
    public string RefId { get; set; }

    [JsonIgnore]
    public DateTimeOffset? ExecutedOn { get; set; }

    //[JsonIgnore]
    //public bool TimeoutOccurred { get; set; }

    [JsonIgnore]
    public IFlowEnvironment Environment => _environment;

    [JsonIgnore]
    internal FlowEnvironment _environment = new();

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
    private IServiceProvider _serviceProvider = null!;

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
        _serviceProvider = services;
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
    /// Override to set all signal handlers
    /// </summary>
    public virtual void RegisterSignals()
    {
        // Example:
        //AddSignalHandler(signal1, handler1);
        //AddSignalHandler(signal2, handler2);
        //AddSignalTimeoutHandler(timeoutHandler);
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

    public virtual void AddSignalTimeoutHandler(Func<SignalPayload, Task> handler)
    {
        AddSignalHandler(FlowBase.TIMEOUT_HANDLER, handler);
    }

    public virtual void AddSignalActivity(string signal, Type activity)
    {
    }

    public virtual void Call(Action action)
    {
        action();
    }

    public virtual async Task CallAsync(Func<Task> action)
    {
        await action();
    }

    // TODO: Activity execution methods - stub implementation
    /// <summary>
    /// Executes an activity with typed method selector (void return)
    /// </summary>
    /// <typeparam name="TActivity">The activity class type</typeparam>
    /// <param name="methodSelector">Expression pointing to the activity method</param>
    /// <param name="options">Activity execution options</param>
    /// <returns>Task representing the activity execution</returns>
    public virtual async Task CallActivity<TActivity>(
        Expression<Func<TActivity, Task>> methodSelector,
        ActivityOptions? options = null)
        where TActivity : class
    {
        // Parse the expression to extract method and parameters
        if (methodSelector.Body is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("Expression must be a method call", nameof(methodSelector));
        }

        // Create activity instance using service provider
        var activityInstance = CreateActivityInstance<TActivity>();

        // Extract parameter values from the expression
        var parameterValues = ExtractParameterValues(methodCall.Arguments);

        // Invoke the method
        var method = methodCall.Method;
        var result = method.Invoke(activityInstance, parameterValues);

        // Handle Task return type
        if (result is Task task)
        {
            await task;
        }
        else
        {
            throw new InvalidOperationException("Activity method must return Task");
        }
    }

    /// <summary>
    /// Executes an activity with typed method selector (with return value)
    /// </summary>
    /// <typeparam name="TActivity">The activity class type</typeparam>
    /// <typeparam name="TResult">The return type</typeparam>
    /// <param name="methodSelector">Expression pointing to the activity method</param>
    /// <param name="options">Activity execution options</param>
    /// <returns>Task representing the activity execution with result</returns>
    public virtual async Task<TResult> CallActivity<TActivity, TResult>(
        Expression<Func<TActivity, Task<TResult>>> methodSelector,
        ActivityOptions? options = null)
        where TActivity : class
    {
        // Parse the expression to extract method and parameters
        if (methodSelector.Body is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("Expression must be a method call", nameof(methodSelector));
        }

        // Create activity instance using service provider
        var activityInstance = CreateActivityInstance<TActivity>();

        // Extract parameter values from the expression
        var parameterValues = ExtractParameterValues(methodCall.Arguments);

        // Invoke the method
        var method = methodCall.Method;
        var result = method.Invoke(activityInstance, parameterValues);

        // Handle Task<TResult> return type
        if (result is Task<TResult> taskWithResult)
        {
            return await taskWithResult;
        }
        else
        {
            throw new InvalidOperationException("Activity method must return Task<TResult>");
        }
    }

    /// <summary>
    /// Creates an instance of the activity using dependency injection
    /// </summary>
    /// <typeparam name="TActivity">The activity type</typeparam>
    /// <returns>Activity instance</returns>
    private TActivity CreateActivityInstance<TActivity>() where TActivity : class
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider not set. Make sure the flow is executed through the FlowEngine.");
        }

        // Try to get from service provider first
        var activityInstance = _serviceProvider.GetService<TActivity>();
        if (activityInstance != null)
        {
            return activityInstance;
        }

        // If not registered in DI, create manually with constructor injection
        try
        {
            var constructorParams = TypeHelper.GetConstructorParameters(_serviceProvider, typeof(TActivity));
            return (TActivity)Activator.CreateInstance(typeof(TActivity), constructorParams)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create activity instance of type {typeof(TActivity).Name}. Make sure it's registered in DI or has a parameterless constructor.", ex);
        }
    }

    /// <summary>
    /// Extracts parameter values from method call expression arguments
    /// </summary>
    /// <param name="arguments">Method call arguments</param>
    /// <returns>Array of parameter values</returns>
    private object?[] ExtractParameterValues(IReadOnlyCollection<Expression> arguments)
    {
        var parameterValues = new object?[arguments.Count];
        var i = 0;

        foreach (var argument in arguments)
        {
            parameterValues[i] = ExtractValueFromExpression(argument);
            i++;
        }

        return parameterValues;
    }

    /// <summary>
    /// Extracts a value from an expression (handles constants, member access, etc.)
    /// </summary>
    /// <param name="expression">Expression to extract value from</param>
    /// <returns>The extracted value</returns>
    private object? ExtractValueFromExpression(Expression expression)
    {
        return expression switch
        {
            ConstantExpression constant => constant.Value,
            MemberExpression member when member.Expression is ConstantExpression memberConstant =>
                GetMemberValue(member, memberConstant.Value),
            MemberExpression member when member.Expression == null => // Static member
                GetMemberValue(member, null),
            _ => throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported for activity parameters. Use constants or simple member access.")
        };
    }

    /// <summary>
    /// Gets the value of a member (field or property)
    /// </summary>
    /// <param name="member">Member expression</param>
    /// <param name="instance">Instance to get the member from (null for static)</param>
    /// <returns>Member value</returns>
    private object? GetMemberValue(MemberExpression member, object? instance)
    {
        return member.Member switch
        {
            FieldInfo field => field.GetValue(instance),
            PropertyInfo property => property.GetValue(instance),
            _ => throw new NotSupportedException($"Member type {member.Member.GetType().Name} is not supported")
        };
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
                //var payload = new SignalPayload() { Value = entry.Record?.Deserialize() };
                var payload = new SignalPayload() { Record = entry.Record };
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
                //var payload = new SignalPayload() { Value = entry.Record?.Deserialize() };
                var payload = new SignalPayload() { Record = entry.Record };
                await _signalHandlers[signalName](payload);
            }

            return;
        }

        throw new FlowStopException("WaitForSignal");
    }

    /// <summary>
    /// Stops flow until signal with signalName received
    /// If signal received then registered signal handler will be triggered to read the payload
    /// If timeoutDate reached before signal received, it passes through
    /// </summary>
    /// <param name="signalName"></param>
    /// <param name="timeout"></param>
    /// <returns>false if timeout reached</returns>
    public virtual async Task WaitForSignalTimeoutAsync(string signalName, TimeSpan timeout)
    {
        _environment.TimeoutOccurred = false;

        // If timeout reached
        var reg = _flowProvider.GetRegisteredFlowTestEnvironment();
        var env = reg.GetFlowTestEnvironment(GetType().FullName, RefId, null);

        if (ExecutedOn != null && ExecutedOn.Value + timeout < DateTimeOffset.UtcNow + env.CurrentDateTimeCorrection)
        {
            if (_signalHandlers.ContainsKey(TIMEOUT_HANDLER))
            {
                var payload = new SignalPayload() 
                { 
                    TimeoutReachedOn = DateTimeOffset.UtcNow,
                    Signal = signalName,
                };

                await _signalHandlers[TIMEOUT_HANDLER](payload);
            }

            _environment.TimeoutOccurred = true;
            return;
        }

        await WaitForSignalAsync(signalName);
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

    public void LoadModelFromParams()
    {
        if (Params?.Payload == null)
        {
            throw new FlowExecutionException("Params.Payload cannot be null");
        }

        var modelSnapshot = JsonSerializer.Deserialize<ModelSnapshot>(Params.Payload);
        modelSnapshot.ExportTo(this);
    }
}
