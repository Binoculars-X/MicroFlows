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
using BlazorForms.Proxyma;
using System.Reflection;
using BlazorForms;
using System.Linq;
using MicroFlows.Domain.Models;

namespace MicroFlows;
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
            Task.Run(() => ProcessCallTaskProxy(invocation.Method.Name, invocation.Arguments)).GetAwaiter().GetResult();
        }
    }

    private async Task InternalInterceptAsynchronous(IInvocation invocation)
    {
        if (!_systemTasks.Contains(invocation.Method.Name) && (invocation.Method.DeclaringType != _runningFlowType || !invocation.Method.IsPublic || !invocation.Method.IsVirtual))
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
            return;
        }
        else
        {
            //_loading = invocation.Method.GetCustomAttribute<LoadTaskAttribute>() != null;
            //_saving = invocation.Method.GetCustomAttribute<SaveTaskAttribute>() != null;
            await ProcessCallTaskProxy(invocation.Method.Name, invocation.Arguments);
        }
    }

    private async Task ProcessCallTaskProxy(string taskName, object[] arguments)
    {
        IFlow model = _context.Model;
        var executionParams = _context.Params;
        _flow.SetModel(model);
        _flow.SetParams(executionParams);
        var method = _flowProxy.GetType().GetMethod(taskName);

        var result = await ProcessCallTask(taskName, async () =>
        {
            if (TypeHelper.IsAsyncMethod(method))
            {
                var task = method.Invoke(_flowProxy, arguments) as Task;
                task.Wait();
            }
            else
            {
                method.Invoke(_flowProxy, arguments);
            }
        });
    }

}
