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

namespace MicroFlows.Application.Engines.Interceptors;
internal partial class InterceptorFlowRunEngine
{
    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        _logger.LogError(new NotImplementedException(), "InterceptAsynchronous<TResult> not required");
        throw new NotImplementedException();
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        if (!_systemTasks.Contains(invocation.Method.Name) && (invocation.Method.DeclaringType != _runningFlowType
            || !invocation.Method.IsPublic || !invocation.Method.IsVirtual))
        {
            invocation.Proceed();
        }
        else
        {
            Task.Run(() => ProcessCallTaskProxy(invocation.Method, invocation.Arguments)).GetAwaiter().GetResult();
        }
    }

    private async Task InternalInterceptAsynchronous(IInvocation invocation)
    {
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            //_loading = invocation.Method.GetCustomAttribute<LoadTaskAttribute>() != null;
            //_saving = invocation.Method.GetCustomAttribute<SaveTaskAttribute>() != null;
            await ProcessCallTaskProxy(invocation.Method, invocation.Arguments);
        }
        else
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
            return;
        }

        //if ( //(invocation.Method.Name == "Execute" && invocation.Method.DeclaringType == _runningFlowType) ||
        //    (!_systemTasks.Contains(invocation.Method.Name) 
        //    && ((invocation.Method.DeclaringType != _runningFlowType && invocation.Method.DeclaringType != typeof(FlowBase)) 
        //    || !invocation.Method.IsPublic || !invocation.Method.IsVirtual)))
        //{
        //    invocation.Proceed();
        //    var task = (Task)invocation.ReturnValue;
        //    await task;
        //    return;
        //}
        //else
        //{
        //    //_loading = invocation.Method.GetCustomAttribute<LoadTaskAttribute>() != null;
        //    //_saving = invocation.Method.GetCustomAttribute<SaveTaskAttribute>() != null;
        //    await ProcessCallTaskProxy(invocation.Method.Name, invocation.Arguments);
        //}
    }

    private async Task ProcessCallTaskProxy(MethodInfo method, object[] arguments)
    {
        string suffix = "";

        if (arguments != null && arguments.Any() && arguments[0] is Func<Task>)
        { 
            suffix = (arguments[0] as Func<Task>).Method.Name;
        }

        string taskName = method.Name + suffix;
        var model = _context.Model;
        var executionParams = _context.Params;
        _flow.SetModel(model);
        _flow.SetParams(executionParams);
        //var method = _flow.GetType().GetMethod(taskName);

        await ProcessCallTask(taskName, async () =>
        {
            if (TypeHelper.IsAsyncMethod(method))
            {
                var task = method.Invoke(_flow, arguments) as Task;
                await task;
                //task.Wait();
            }
            else
            {
                method.Invoke(_flow, arguments);
            }
        });
    }

}
