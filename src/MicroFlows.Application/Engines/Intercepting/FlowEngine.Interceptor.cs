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
internal partial class FlowEngine
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
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            //_loading = invocation.Method.GetCustomAttribute<LoadTaskAttribute>() != null;
            //_saving = invocation.Method.GetCustomAttribute<SaveTaskAttribute>() != null;
            _callingContext = invocation.Method.Name;
            Task.Run(() => ProcessCallTaskProxy(invocation.Method, invocation.Arguments)).GetAwaiter().GetResult();
        }
        else
        {
            invocation.Proceed();
        }

        //if (!_systemTasks.Contains(invocation.Method.Name) && (invocation.Method.DeclaringType != _runningFlowType
        //    || !invocation.Method.IsPublic || !invocation.Method.IsVirtual))
        //{
        //    invocation.Proceed();
        //}
        //else
        //{
        //    Task.Run(() => ProcessCallTaskProxy(invocation.Method, invocation.Arguments)).GetAwaiter().GetResult();
        //}
    }

    private async Task InternalInterceptAsynchronous(IInvocation invocation)
    {
        if ((invocation.Method.DeclaringType == _runningFlowType || invocation.Method.DeclaringType == typeof(FlowBase))
            && invocation.Method.IsPublic && invocation.Method.IsVirtual)
        {
            //_loading = invocation.Method.GetCustomAttribute<LoadTaskAttribute>() != null;
            //_saving = invocation.Method.GetCustomAttribute<SaveTaskAttribute>() != null;
            _callingContext = invocation.Method.Name;
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
        string taskName = method.Name;

        if (arguments != null && arguments.Any() && arguments[0] is Func<Task>)
        {
            var parameterMethodName = (arguments[0] as Func<Task>).Method.Name;
            
            if (parameterMethodName.Contains(">b__"))
            {
                taskName += "_Anonymus";
            }
            else
            {
                taskName += $"_{parameterMethodName}";
            }
        }

        var model = _context.Model;
        var executionParams = _context.Params;
        _targetFlow.SetModel(model);

        // ToDo: I guess it is enough to set parameters only once at the moment where we start flow
        //_targetFlow.SetParams(executionParams);
        //var method = _flow.GetType().GetMethod(taskName);

        await ProcessCallTask(taskName, async () =>
        {
            if (TypeHelper.IsAsyncMethod(method))
            {
                var task = method.Invoke(_targetFlow, arguments) as Task;
                await task;
                //task.Wait();
            }
            else
            {
                method.Invoke(_targetFlow, arguments);
            }
        });
    }

}
