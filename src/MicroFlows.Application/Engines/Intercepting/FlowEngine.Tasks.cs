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
using System.Runtime.CompilerServices;
using MicroFlows.Application.Exceptions;
using MicroFlows.Domain.Enums;

namespace MicroFlows.Application.Engines.Interceptors;
internal partial class FlowEngine
{
    private async Task ProcessCallTask(string taskName, Func<Task> action)
    {
        _callIndex++;
        string taskNameId = $"{taskName}:{_callIndex}";
        _executionCallStack.Add(taskNameId);

        // check if we have context for that step in the storage
        FlowContext currentTaskContext = GetTaskExecutionContextFromHistory(_context.RefId, taskNameId, _callIndex) ?? _context;
        // source context must not be changed, because it is in the cache
        currentTaskContext = TypeHelper.CloneObject(currentTaskContext);
        _context = currentTaskContext;

        bool isSkipMode = CompareExecutionCallStacksIdentical();

        if (isSkipMode)
        {
            // execute skip task - supply model that was on this step
            _flowProxy.SetModel(currentTaskContext.Model);

            // ToDo: I guess it is enough to set parameters only once at the moment where we start flow
            //_flowProxy.SetParams(currentTaskContext.Params);
        }
        else
        {
            // execute task

            // if flow changed model we should inherit this change
            // we make sure that model is a new instance
            _context.Model.ImportFrom(_flowProxy);
            var result = await ExecuteTask(action);

            if (result.ResultState != ResultStateEnum.Fail)
            {
                // ToDo: we should copy model to context and sttre it 
                _context.Model.ImportFrom(_flowProxy);
                //if (taskName.Contains("_Anonymus"))
                //{
                //    // anonimous delegate
                //}
                //else
                //{
                //    // virtual method
                //    _context.Model.ImportFrom(_targetFlow);
                //    _context.Model.ImportFrom(_flowProxy);
                //}
            }

            // After Task Events
            //TriggerEvents(taskName);

            var continueExecution = CanContinueExecution(result);

            if (continueExecution)
            {
                _context.CallStack.Add(taskNameId);
            }

            _context.CurrentTask = taskNameId;
            _context.ExecutionResult = result;

            // add context to history
            await AddContextToHistory(_context);

            if (!continueExecution)
            {
                _logger.LogInformation("ProcessCallTask - execution stopped");
                throw new FlowStopException();
            }
        }
    }

    private async Task<TaskExecutionResult> ExecuteTask(Func<Task> action)
    {
        var result = new TaskExecutionResult
        {
            ResultState = ResultStateEnum.Success,
            FlowState = FlowStateEnum.Continue
        };

        // clear _context
        if (_context.ExecutionResult != null)
        {
            _context.ExecutionResult.IsFormTask = false;
        }

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
                result.ExceptionType = exc.GetType().Name;
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
                result.ExceptionStackTrace = innerExc.StackTrace;
                result.ExceptionType = innerExc.GetType().Name;
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
            result.ExceptionType = exc.GetType().Name;
            _logger.LogInformation(exc, $"Flow '{_runningFlowType}' stopped");
        }
        catch (FlowFailedException exc)
        {
            LogException(exc);
            result.ResultState = ResultStateEnum.Fail;
            result.FlowState = FlowStateEnum.Stop;
            result.ExceptionMessage = exc.Message;
            result.ExceptionStackTrace = exc.StackTrace;
            result.ExceptionType = exc.GetType().Name;
            LogException(exc);
        }
        catch (Exception exc)
        {
            LogException(exc);
            result.ResultState = ResultStateEnum.Fail;
            result.FlowState = FlowStateEnum.Stop;
            result.ExceptionMessage = exc.Message;
            result.ExceptionStackTrace = exc.StackTrace;
            result.ExceptionType = exc.GetType().Name;
            LogException(exc);
        }
        finally
        {
            // populate formId from formPlugin
            if (_context?.ExecutionResult?.IsFormTask == true)
            {
                result.FormId = _context.ExecutionResult.FormId;
                result.CallbackTaskId = _context.ExecutionResult.CallbackTaskId;
                result.FormState = _context.ExecutionResult.FormState;
                result.IsFormTask = _context.ExecutionResult.IsFormTask;
            }

            // populate rule validations
            //result.TaskExecutionValidationIssues = _context.ExecutionResult.TaskExecutionValidationIssues;
        }

        return result;
    }

    private void TriggerEvents(string taskName)
    {
        //if (_loading)
        //{
        //    OnLoad?.Invoke(_flowBase, new FlowEventArgs { TaskName = taskName, Context = _context, Model = _context.Model });
        //    _loading = false;
        //}

        //if (_saving)
        //{
        //    OnSave?.Invoke(_flowBase, new FlowEventArgs { TaskName = taskName, Context = _context, Model = _context.Model });
        //    _saving = false;
        //}
    }

    private bool CanContinueExecution(TaskExecutionResult result)
    {
        return result.ResultState != ResultStateEnum.Fail && result.FlowState == FlowStateEnum.Continue;
    }


    private void LogException(Exception exc)
    {
        Console.WriteLine($"Exception thrown, message: {exc.Message}, stackTrace: {exc.StackTrace}");

        if (exc is AggregateException)
        {
            foreach (var e in (exc as AggregateException).InnerExceptions)
            {
                LogException(e);
            }
        }
        else if (exc is TargetInvocationException)
        {
            LogException(exc.InnerException);
        }
        else
        {
            if (exc.InnerException != null)
            {
                LogException(exc.InnerException);
            }
        }
    }

    private bool CompareExecutionCallStacksIdentical()
    {
        var left = _context.CallStack.Distinct().ToList();
        var right = _executionCallStack.Distinct().ToList();

        for (int i = 0; i < right.Count(); i++)
        {
            if (left.Count() <= i || right[i] != left[i])
            {
                return false;
            }
        }

        return true;
    }

    
}
