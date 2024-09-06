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

namespace MicroFlows.Application.Engines.Interceptors;
    
// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
internal partial class InterceptorFlowRunEngine : IAsyncInterceptor, IFlowRunEngine
{
    private readonly ILogger<InterceptorFlowRunEngine> _logger;
    private readonly IServiceProvider _services;
    //private readonly IProxymaProvider _proxyProvider;
    private readonly IProxyGenerator _proxyGenerator;
    private readonly IFlowRepository _flowRepository;

    // running flow state
    private FlowBase? _flow;
    private FlowBase? _flowProxy;
    private Type? _runningFlowType;
    private string[] _systemTasks = ["Begin", "BeginAsync", "EndAsync"];
    private FlowContext _context;
    private int _callIndex;
    private List<string> _executionCallStack;
    private List<FlowContext> _contextHistory;
    private FlowParams? _flowParams;

    public InterceptorFlowRunEngine(ILogger<InterceptorFlowRunEngine> logger, 
        IServiceProvider serviceProvider,
        //IProxymaProvider proxyProvider,
        IProxyGenerator proxyGenerator,
        IFlowRepository flowRepository)
    {
        _logger = logger;
        _services = serviceProvider;
        //_proxyProvider = proxyProvider;
        _proxyGenerator = proxyGenerator;
        _flowRepository = flowRepository;
    }

    public async Task<FlowContext> ExecuteFlow(string flowTypeName, FlowParams? flowParams = null)
    {
        var type = TypeHelper.ResolveType(flowTypeName);
        return await ExecuteFlow(type, flowParams);
    }

    /// <summary>
    /// Flow execution steps:
    /// 1. Create _targetFlow
    /// 2. Create _flowProxy
    /// 3. Create _context
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
        // construct flow
        _flow = _services.GetService(flowType) as FlowBase;
        _runningFlowType = flowType;
        _flowParams = flowParams ?? new FlowParams();

        var options = new ProxyGenerationOptions(new FreezableProxyGenerationHook(_flow));
        var flowParameters = TypeHelper.GetConstructorParameters(_services, flowType);

        try
        {
            //_flowProxy = _proxyProvider.CreateClassProxy(flowType, flowParameters, _flow, options, [this]) as FlowBase;
            //_flowProxy = _proxyGenerator.CreateClassProxyWithTarget(flowType, _flow, options, [this]) as FlowBase;
            // classToProxy: classToProxy, constructorArguments: constructorArguments, target: target, options: options, interceptors: interceptors
            
            _flowProxy = _proxyGenerator.CreateClassProxyWithTarget(classToProxy: flowType, 
                constructorArguments: flowParameters,
                target: _flow,
                options: options,
                interceptors: [this]) as FlowBase;
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "CreateClassProxy failed");
            throw;
        }

        string refId;

        if (_flowParams.RefId == null)
        {
            // prepare context
            _context = await _flowRepository.CreateFlowContext(_flow, _flowParams);
            //flowRunId = _context.FlowRunId;
            refId = _context.RefId;
        }
        else
        {
            refId = _flowParams.RefId;
            _context = (await _flowRepository.GetFlowHistory(refId)).First();
        }

        _flowProxy.SetParams(_flowParams);
        

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
        //_flowProxy.SetTaskExecutor(this);

        // execute flow
        try
        {
            //await _flowProxy.Execute();
            var method = _flowProxy.GetType().GetMethod("Flow");
            var task = (Task)method.Invoke(_flowProxy, null);
            await task;
            //var task = (Task)method.Invoke(_flowProxy, null);
            //task.Wait();

            // flow executed completely
            _context.ExecutionResult.FlowState = FlowStateEnum.Finished;
            _context.ExecutionResult.ResultState = ResultStateEnum.Success;
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
                //_context.ExecutionResult.ExecutionException = exc;
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
            //_context.ExecutionResult.ExecutionException = exc;
            LogException(exc);
        }
        catch (FlowFailedException exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            _context.ExecutionResult.FlowState = FlowStateEnum.Finished;
            _context.ExecutionResult.ExceptionMessage = exc.Message;
            _context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            _context.ExecutionResult.ExceptionType = exc.GetType().Name;
            //_context.ExecutionResult.ExecutionException = exc;
            LogException(exc);
        }
        catch (Exception exc)
        {
            _context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            _context.ExecutionResult.ExceptionMessage = exc.Message;
            _context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            _context.ExecutionResult.ExceptionType = exc.GetType().Name;
            //_context.ExecutionResult.ExecutionException = exc;
            LogException(exc);
        }
        finally
        {
                // save flow if success, stop, failure
            await AddContextToHistory(_context);

            if (_flowParams.FlowOptions.NoStorage == false)
            {
                await _flowRepository.SaveContextHistory(_contextHistory);
            }

            // it should be refreshed from database next time
            _contextHistory = null;
        }

        return _context;
    }

    public Task<FlowContext> ResumeFlow(string flowId)
    {
        throw new NotImplementedException();
    }

    
}
