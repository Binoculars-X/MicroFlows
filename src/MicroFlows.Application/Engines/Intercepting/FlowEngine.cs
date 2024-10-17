using Castle.DynamicProxy;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Application.Engines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using MicroFlows.Domain.Models;
using Microsoft.CodeAnalysis;
using MicroFlows.Application.Exceptions;
using MicroFlows.Domain.Enums;
using JsonPathToModel;

namespace MicroFlows.Application.Engines.Interceptors;
    
/// <summary>
/// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
/// </summary>
internal partial class FlowEngine : IAsyncInterceptor, IFlowEngine
{
    private readonly ILogger<FlowEngine> _logger;
    private readonly IServiceProvider _services;
    private readonly IProxyGenerator _proxyGenerator;
    private readonly IFlowRepository _flowRepository;

    // running flow state
    private FlowBase? _targetFlow;
    private FlowBase? _flowProxy;
    private Type? _runningFlowType;
    private string[] _systemTasks = ["Begin", "BeginAsync", "EndAsync"];
    private FlowContext _context;
    private string _callingContext;
    private int _callIndex;
    private List<string> _executionCallStack;
    private List<FlowContext>? _contextHistory;
    private FlowParams? _flowParams;
    private Dictionary<string, object?> _signals = [];

    public const string FLOW_METHOD = "Flow";

    public FlowEngine(ILogger<FlowEngine> logger, 
        IServiceProvider serviceProvider,
        IProxyGenerator proxyGenerator,
        IFlowRepository flowRepository)
    {
        _logger = logger;
        _services = serviceProvider;
        _proxyGenerator = proxyGenerator;
        _flowRepository = flowRepository;
    }

    public async Task<FlowContext> SendSignal(Type flowType, string signal, FlowParams? flowParams = null, object? payload = null)
    {
        _signals[signal] = payload;
        return await ExecuteFlow(flowType, flowParams);
    }

    public async Task<FlowContext> SendSignals(Type flowType, IDictionary<string, object?> signals, FlowParams? flowParams = null)
    {
        _signals = new Dictionary<string, object?>(signals);
        return await ExecuteFlow(flowType, flowParams);
    }

    /// <summary>
    /// Flow execution steps:
    /// 1. Create _targetFlow, _flowProxy, _context
    /// 2. Set RefId and SetParams
    /// 3. Merge SignalJournal with new supplied signals
    /// 4. Invoke [Flow] method of _flowProxy using reflection
    /// 5. All virtual Call methods are intercepted, but other calls proceeded to _targetFlow:
    ///   - ProcessCallTaskProxy gets model snapshot from _context and sets to _targetFlow
    ///   - then executes ProcessCallTask supplying a delegate that invokes the Call method on _targetFlow
    ///     - ProcessCallTask checks if it is in Skip Mode:
    ///       - In Skip Mode it reads corresponding to the current step Context and updates _flowProxy model from it
    ///       - Returns without execution of the supplied delegate
    ///     - In Normal Mode ProcessCallTask imports _flowProxy model to _context 
    ///     - Executes ExecuteTask supplying the delegate
    ///       - ExecuteTask invokes the supplied delegate (Call method implemented in FlowBase)
    ///         - FlowBase Call method triggered with another delegate - an argument of the Call method
    ///         - Call method invokes the argument delegate, which is invoked in _flowProxy, catches exceptions
    ///           - _flowProxy methods change _flowProxy Model !!! 
    ///           *** All Model changes always happen on _flowProxy, and never on _targetFlow ***
    ///     - ProcessCallTask checks result
    ///       - If no failure it imports _flowProxy Model to _context Model
    ///     - Saves Context history and call stack
    ///     - throws FlowStopException if the flow stopped
    /// 6. Continues execution of [Flow] method, that can trigger interceptors by Call methods, then step 5. repeated
    /// 7. Await [Flow] method finish and catch exceptions
    /// 8. Updates flow _context state
    /// 9. Saves flow _context
    /// </summary>
    /// <param name="flowType"></param>
    /// <param name="flowParams"></param>
    /// <returns></returns>
    public async Task<FlowContext> ExecuteFlow(Type flowType, FlowParams? flowParams = null)
    {
        _flowParams = flowParams ?? new FlowParams();
        _flowParams.FlowType = flowType;

        if (MicroFlowsConfigurationServices.IsFluentFlow(flowType))
        {
            return await ExecuteFluentFlow(flowParams);
        }

        // construct flow
        _targetFlow = _services.GetService(flowType) as FlowBase;

        if (_targetFlow == null)
        {
            throw new FlowValidationException($"Flow of type '{flowType}' is not registered");
        }

        _runningFlowType = flowType;

        var options = new ProxyGenerationOptions(new FreezableProxyGenerationHook(_targetFlow));
        var flowParameters = TypeHelper.GetConstructorParameters(_services, flowType);

        try
        {
            _flowProxy = _proxyGenerator.CreateClassProxyWithTarget(classToProxy: flowType,
                constructorArguments: flowParameters,
                target: _targetFlow,
                options: options,
                interceptors: [this]) as FlowBase;

            if (_flowProxy == null)
            {
                throw new Exception($"Cannot create proxy from Flow '{flowType.FullName}'");
            }
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "CreateClassProxy failed");
            throw;
        }

        var refId = await FindOrCreateContext();
        _targetFlow.RefId = refId;
        _flowProxy.RefId = refId;

        _targetFlow.SetParams(_flowParams);
        _flowProxy.SetParams(_flowParams);

        _targetFlow.SetServiceProvider(_services);
        _flowProxy.SetServiceProvider(_services);

        // signals
        await UpdateSignalJournal();

        if (_flowParams.FlowOptions.NoStorage)
        {
            // dummy history
            _contextHistory = new List<FlowContext>();
            await AddContextToHistory(_context);
        }
        else
        {
            await PreloadContextHistory(refId);
        }

        _executionCallStack = new List<string>();
        _callIndex = 0;

        // execute flow
        try
        {
            var method = GetFlowActivationMethod();
            var task = (Task)method.Invoke(_flowProxy, null);
            await task;

            // flow executed completely
            _context.ExecutionResult.FlowState = FlowStateEnum.Finished;
            _context.ExecutionResult.ResultState = ResultStateEnum.Success;
            SaveEndContext();
        }
        catch (TargetInvocationException exc)
        {
            LogException(exc);
            var innerExc = exc.InnerException as FlowStopException;

            if (innerExc != null)
            {
                _context.ExecutionResult.ResultState = ResultStateEnum.Success;
                _context.ExecutionResult.FlowState = FlowStateEnum.Stop;
                _context.ExecutionResult.ExceptionMessage = innerExc.Message;
                _context.ExecutionResult.ExceptionStackTrace = innerExc.StackTrace;
                _context.ExecutionResult.ExceptionType = innerExc.GetType().Name;
            }
            else
            {
                throw;
            }
        }
        catch (FlowStopException exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Success;
            _context.ExecutionResult.FlowState = FlowStateEnum.Stop;
            _context.ExecutionResult.ExceptionMessage = exc.Message;
            _context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            _context.ExecutionResult.ExceptionType = exc.GetType().Name;
            LogException(exc);
        }
        catch (FlowTaskFailedException exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            _context.ExecutionResult.FlowState = FlowStateEnum.Stop;
            // preserve original exception details
            //_context.ExecutionResult.ExceptionMessage = exc.Message;
            //_context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            //_context.ExecutionResult.ExceptionType = exc.GetType().Name;
            LogException(exc);

            // if exception happened inside delegate method body we should save it
            await SaveFailedContext();
        }
        catch (FlowFailedException exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            _context.ExecutionResult.FlowState = FlowStateEnum.Finished;
            _context.ExecutionResult.ExceptionMessage = exc.Message;
            _context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            _context.ExecutionResult.ExceptionType = exc.GetType().Name;
            LogException(exc);

            // if exception happened inside Flow method body we should save it
            _context.CurrentTask = null;
            await SaveFailedContext();
        }
        catch (NonDeterministicFlowException exc)
        {
            // Fatal error should be bubled up
            LogException(exc);
            throw;
        }
        catch (Exception exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            _context.ExecutionResult.FlowState = FlowStateEnum.Stop;
            _context.ExecutionResult.ExceptionMessage = exc.Message;
            _context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            _context.ExecutionResult.ExceptionType = exc.GetType().Name;
            LogException(exc);

            // if exception happened inside Flow method body we should save it
            _context.CurrentTask = null;
            await SaveFailedContext();
        }
        finally
        {
            // save flow if success, stop, failure
            // ToDo: check why we save it here if it is already saved in ProcessCallTask
            //await AddContextToHistory(_context);

            if (_flowParams.FlowOptions.NoStorage == false)
            {
                await _flowRepository.SaveContextHistory(_contextHistory);
            }

            // it should be refreshed from database next time
            _contextHistory = null;
        }

        return _context;
    }

    private async Task UpdateSignalJournal()
    {
        foreach (var signal in _signals)
        {
            _flowProxy!.SignalJournal.Add(new SignalJournalEntry(signal.Key, signal.Value));
        }

        var flowStoreModel = await _flowRepository.UpdateFlow(_flowProxy!);
        _flowProxy!.SignalJournal = flowStoreModel.SignalJournal;
        _targetFlow!.SignalJournal = flowStoreModel.SignalJournal;
    }

    private async Task<string> FindOrCreateContext()
    {
        var model = await _flowRepository.FindFlowHistory(new FlowSearchQuery(_flowParams.RefId, _flowParams.ExternalId));

        if (model == null)
        {
            _context = await _flowRepository.CreateFlowContext(_targetFlow, _flowParams);

            // The first task is always Begin, the last task is always End
            _context.CurrentTask = $"{TaskDefTypes.Begin}:{_callIndex}";
        }
        else
        {
            _context = model.First();
        }

        return _context.RefId;
    }

    private async Task SaveFailedContext()
    {        
        await AddContextToHistory(_context);
    }

    private async Task SaveEndContext()
    {
        // The first task is always Begin, the last task is always End
        _context.Model.ImportFrom(_flowProxy, _importOptions);
        _context.CurrentTask = $"{TaskDefTypes.End}:{_callIndex + 1}";
        await AddContextToHistory(_context);
    }

    private MethodInfo GetFlowActivationMethod()
    {
        var methods = _flowProxy.GetType().GetMethods()
            .Where(m => m.Name == FLOW_METHOD && m.GetParameters().Length == 0 && m.ReturnType == typeof(Task));
        
        // ToDo: may be add another way to define Flow method, by attributes or expression
        return methods.FirstOrDefault();
    }
   
}
