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
using JsonPathToModel;
using MicroFlows.Application.Engines.Intercepting;

namespace MicroFlows.Application.Engines.Interceptors;
internal partial class FlowEngine
{
    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        var result = InternalInterceptAsynchronous(invocation);

        if (result.Status == TaskStatus.RanToCompletion)
        {
            //Task<TResult> cast = Task.FromResult((TResult)result.StealValue("$.Result"));
            invocation.ReturnValue = result.StealValue("$.Result");
            return;
        }

        var convertedResult = new TaskConverter2(typeof(TResult)).ConvertReturnType(result);
        invocation.ReturnValue = convertedResult;
        return;

        var x = result.ContinueWith<TResult>(x => (TResult)x.Result);
        invocation.ReturnValue = x;
        //if (result.Status == TaskStatus.RanToCompletion)
        //{
        //    Task<TResult> cast = Task.FromResult((TResult)result.StealValue("$.Result"));
        //    invocation.ReturnValue = cast;
        //    return;
        //}

        //invocation.ReturnValue = result;

        //_logger.LogError(new NotImplementedException(), "InterceptAsynchronous<TResult> not required");
        //throw new NotImplementedException();
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            Task.Run(async () =>
            {
                await ProcessCallTaskProxy(invocation.Method, invocation.Arguments);
                //var result = await ProcessCallTaskProxy(invocation.Method, invocation.Arguments);
                //invocation.ReturnValue = result;
            }).GetAwaiter().GetResult();
        }
        else
        {
            invocation.Proceed();
        }
    }

    private async Task<object> InternalInterceptAsynchronous(IInvocation invocation)
    {
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            var result = await ProcessCallTaskProxy(invocation.Method, invocation.Arguments);
            return result;
        }
        else
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
            return task;
        }
    }

    private async Task<object> ProcessCallTaskProxy(MethodInfo method, object[] arguments)
    {
        string taskName = GenerateTaskName(method, arguments);

        var model = _context.Model;
        var executionParams = _context.Params;
        _targetFlow.SetModel(model);

        // ToDo: I guess it is enough to set parameters only once at the moment where we start flow
        //_targetFlow.SetParams(executionParams);
        //var method = _flow.GetType().GetMethod(taskName);

        return await ProcessCallTask(taskName, async () =>
        {
            if (TypeHelper.IsAsyncMethod(method))
            {
                //return await (method.Invoke(_targetFlow, arguments) as Task);
                var task = method.Invoke(_targetFlow, arguments) as Task;
                await task!;
                return task;
                //return task.StealValue("$.Result");
            }
            else
            {
                return Task.FromResult(method.Invoke(_targetFlow, arguments));
            }
        });
    }

    private static string GenerateTaskName(MethodInfo method, object[] arguments)
    {
        string taskName = method.Name;

        if (arguments != null && arguments.Any())
        {
            if (arguments[0] is Func<Task>)
            {
                var parameterMethodName = ((Func<Task>)arguments[0]).Method.Name;

                if (parameterMethodName.Contains(">b__"))
                {
                    taskName += "_Anonymous";
                }
                else
                {
                    taskName += $"_{parameterMethodName}";
                }
            }
            else if (arguments[0] is Action)
            {
                taskName += $"_{((Action)arguments[0]).Method.Name}";
            }

        }

        return taskName;
    }
}
