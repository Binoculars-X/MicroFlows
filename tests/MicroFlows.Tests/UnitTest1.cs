using Castle.DynamicProxy;
using MicroFlows.Application.Engines;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Options;
using System.Reflection;
using MicroFlows.Application.Engines.Interceptors;

namespace MicroFlows.Tests;

public class UnitTest1 : IAsyncInterceptor
{
    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        throw new NotImplementedException();
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        throw new NotImplementedException();
    }

    private Type? _runningFlowType;
    private FlowBase? _flowProxy;
    ModelSnapshot _model = new();

    [Fact]
    public async Task Test1()
    {
        var proxyGenerator = new ProxyGenerator();
        _runningFlowType = typeof(SampleFlow);
        var options = new ProxyGenerationOptions(new FreezableProxyGenerationHook(new SampleFlow()));

        _flowProxy = proxyGenerator.CreateClassProxy(classToProxy: _runningFlowType,
                constructorArguments: [],
                //target: _flow,
                options: options,
                interceptors: [this]) as FlowBase;

        try
        {
            //await _flowProxy.Execute();
            var method = _flowProxy.GetType().GetMethod("Flow");
            var task = (Task)method.Invoke(_flowProxy, null);
            await task;
        }
        catch (Exception ex)
        {

        }
    }

    private async Task InternalInterceptAsynchronous(IInvocation invocation)
    {
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            await ProcessCallTaskProxy(invocation.Method, invocation.Arguments);
        }
        else
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
            return;
        }
    }

    private async Task ProcessCallTaskProxy(MethodInfo method, object[] arguments)
    {
        string suffix = "";

        if (arguments != null && arguments.Any() && arguments[0] is Func<Task>)
        {
            suffix = (arguments[0] as Func<Task>).Method.Name;
        }

        string taskName = method.Name + suffix;
        _flowProxy.SetModel(_model);

        await ProcessCallTask(taskName, async () =>
        {
            if (TypeHelper.IsAsyncMethod(method))
            {
                var task = method.Invoke(_flowProxy, arguments) as Task;
                await task;
                //task.Wait();
            }
            else
            {
                method.Invoke(_flowProxy, arguments);
            }
        });
    }

    private async Task ProcessCallTask(string taskName, Func<Task> action)
    {
        // execute task

        // if flow changed model we should inherit this change
        // we make sure that model is a new instance
        _model.ImportFrom(_flowProxy);
        var result = await ExecuteTask(action);
        _model.ImportFrom(_flowProxy);

        var continueExecution = result.ResultState != ResultStateEnum.Fail && result.FlowState == FlowStateEnum.Continue;

        if (!continueExecution)
        {
            throw new FlowStopException();
        }
        
    }

    private async Task<TaskExecutionResult> ExecuteTask(Func<Task> action)
    {
        var result = new TaskExecutionResult
        {
            ResultState = ResultStateEnum.Success,
            FlowState = FlowStateEnum.Continue
        };

        try
        {
            await action();
        }
        catch (AggregateException exc)
        {
            LogException(exc);
            var innerExc = exc.InnerException;

            if (innerExc != null)
            {
                result.ResultState = ResultStateEnum.Fail;
                result.ExceptionMessage = exc.Message;
                result.ExceptionStackTrace = exc.StackTrace;
            }
        }
        catch (TargetInvocationException exc)
        {
            LogException(exc);
            var innerExc = exc.InnerException as FlowStopException;

            if (innerExc != null)
            {
                result.ResultState = ResultStateEnum.Success;
                result.FlowState = FlowStateEnum.Stop;
                result.ExceptionMessage = innerExc.Message;
                result.ExceptionStackTrace = exc.StackTrace;
            }
            else
            {
                throw;
            }
        }
        catch (FlowStopException exc)
        {
            result.ResultState = ResultStateEnum.Success;
            result.FlowState = FlowStateEnum.Stop;
            result.ExceptionMessage = exc.Message;
            result.ExceptionStackTrace = exc.StackTrace;
        }
        catch (FlowFailedException exc)
        {
            LogException(exc);
            result.ResultState = ResultStateEnum.Fail;
            result.FlowState = FlowStateEnum.Stop;
            result.ExceptionMessage = exc.Message;
            result.ExceptionStackTrace = exc.StackTrace;
            LogException(exc);
        }
        catch (Exception exc)
        {
            LogException(exc);
            result.ResultState = ResultStateEnum.Fail;
            result.ExceptionMessage = exc.Message;
            result.ExceptionStackTrace = exc.StackTrace;
            LogException(exc);
        }
        finally
        {
        }

        return result;
    }

    private void LogException(Exception exc)
    { }
}

