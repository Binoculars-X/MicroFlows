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
    
// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
internal partial class FlowEngine
{
    public virtual async Task<FlowContext> CreateFluentFlow(FlowParams? runParameters)
    {
        var flowType = runParameters.FlowType;
        var refId = runParameters.RefId;
        FlowContext? context = null;

        var flow = _services.GetService(flowType) as FlowBase;

        if (flow == null)
        {
            throw new FlowValidationException($"Flow of type '{flowType}' is not registered");
        }

        var flowBuilder = new FluentFlowBuilder(flow);
        flowBuilder.Parse();

        if (string.IsNullOrEmpty(refId))
        {
            context = await _flowRepository.CreateFlowContext(flow, runParameters);
            context.ExecutionResult = new TaskExecutionResult();
            _contextHistory = new List<FlowContext>();
        }
        else
        {
            var _contextHistory = await _flowRepository.FindFlowHistory(new FlowSearchQuery(_flowParams.RefId,
                _flowParams.ExternalId));

            if (_contextHistory == null)
            {
                context = await _flowRepository.CreateFlowContext(flow, runParameters);
                context.RefId = refId;
                _contextHistory = new List<FlowContext>();
            }
            else
            {
                context = _contextHistory.First();
            }


            context.ExecutionResult.FlowState = FlowStateEnum.Continue;
            context.ExecutionResult.ResultState = ResultStateEnum.Success;
        }

        return context;
    }

    public virtual async Task<FlowContext> ExecuteFluentFlow(FlowParams? runParameters)
    {
        _flowParams = _flowParams ?? runParameters;
        var flowType = runParameters.FlowType;
        var refId = runParameters.RefId;
        //var parameters = runParameters.FlowParameters;
        //var noStorage = runParameters.NoStorageMode;
        //var context = runParameters.Context;
        FlowContext? context = null;

        var flow = _services.GetService(flowType) as FlowBase;

        if (flow == null)
        {
            throw new FlowValidationException($"Flow of type '{flowType}' is not registered");
        }

        //flow.Parse();
        var flowBuilder = new FluentFlowBuilder(flow);
        flowBuilder.Parse();
        //flow.Define(flowBuilder);
        //flow.Tasks = flowBuilder.Tasks;
        
        //flow.SetFirstPass(runParameters.FirstPass);

        if (context == null)
        {
            if (string.IsNullOrEmpty(refId))
            {
                //context = await _storage.CreateProcessExecutionContext(flow, parameters, noStorage);
                context = await _flowRepository.CreateFlowContext(flow, runParameters);
                context.ExecutionResult = new TaskExecutionResult();
                _contextHistory = new List<FlowContext>();
            }
            else
            {
                _contextHistory = await _flowRepository.FindFlowHistory(new FlowSearchQuery(_flowParams.RefId, 
                    _flowParams.ExternalId));

                if (_contextHistory == null)
                {
                    context = await _flowRepository.CreateFlowContext(flow, runParameters);
                    context.RefId = refId;
                    _contextHistory = new List<FlowContext>();
                }
                else
                {
                    context = _contextHistory.First();
                }

                //context = await _storage.GetProcessExecutionContext(refId);

                //if (context == null)
                //{
                //    context = await _storage.CreateProcessExecutionContext(flow, parameters, true);
                //    context.RefId = refId;
                //    await _storage.SaveProcessExecutionContext(context, context.ExecutionResult, true);
                //}

                context.ExecutionResult.FlowState = FlowStateEnum.Continue;
                context.ExecutionResult.ResultState = ResultStateEnum.Success;
            }
        }
        //else if (context != null && parameters != null)
        //{
        //    // merge parameters from runParameters
        //    var di = context.Params.ConcatDynamic(parameters);
        //    context.Params = parameters;
        //    context.Params.DynamicInput = di;
        //}

        //flow.SetFlowRefId(context.RefId);
        //flow.SetFlowContext(context);

        var index = context.CurrentTaskLine;
        context.ExecutionResult.FlowState = FlowStateEnum.Continue;
        context.ExecutionResult.ResultState = ResultStateEnum.Success;
        flow.SetServiceProvider(_services);
        flow.ExecutedOn = context.CreatedOn;
        flow.SetParams(_flowParams);
        flow.SetModel(context.Model);
        flow.RefId = context.RefId;

        // update signals
        flow.RegisterSignals();

        foreach (var signal in _signals)
        {
            flow.SignalJournal.Add(
                new SignalJournalEntry(signal.Key, signal.Value) { Received = DateTimeOffset.UtcNow });
        }

        if (!flow.Params.FlowOptions.NoStorage)
        {
            var flowStoreModel = await _flowRepository.UpdateFlow(flow);
            flow.SignalJournal = flowStoreModel.SignalJournal;
        }

        await RunFlowTasks(index, flow, context, flowBuilder);
        return context;
    }

    private static async Task ExecuteTask(TaskDetails task, IFlow flow, FlowContext context)
    {
        try
        {
            if (task.Action != null)
            {
                await task.Action();
            }
            else if (task.NonAsyncAction != null)
            {
                task.NonAsyncAction();
            }

            //context.Model = flow.GetModel();
            context.Model.ImportFrom(flow);
            context.ExecutionResult.ResultState = ResultStateEnum.Success;
            context.ExecutionResult.FlowState = FlowStateEnum.Continue;
        }
        catch (Exception exc)
        {
            context.ExecutionResult.ResultState = ResultStateEnum.Fail;
            context.ExecutionResult.ExceptionMessage = exc.Message;
            context.ExecutionResult.ExceptionStackTrace = exc.StackTrace;
            context.ExecutionResult.ExceptionType = exc.GetType().FullName;
        }
    }

    public const int MAX_LOOP_COUNT = 1000;

    private async Task RunFlowTasks(int index, FlowBase flow, FlowContext context, IFlowBuilder flowBuilder)
    {
        var settings = flow.Params.FlowOptions;
        var i = index;
        var currentIteration = 0;

        while (i < flowBuilder.Tasks.Count)
        {
            if (context.ExecutionResult.ResultState == ResultStateEnum.Fail ||
                context.ExecutionResult.FlowState == FlowStateEnum.Stop ||
                context.ExecutionResult.FlowState == FlowStateEnum.Waiting ||
                context.ExecutionResult.FlowState == FlowStateEnum.Finished)
            {
                await AddContextToHistory(context);

                // ToDo: refactor to use AddContextToHistory/_flowRepository.SaveContextHistory
                // save context and stop if flow settings NoStoreTillStop
                if (settings.StoreModel == FlowExecutionStoreModel.NoStoreTillStop &&
                    context.ExecutionResult.FlowState == FlowStateEnum.Stop)
                {
                    //await _storage.SaveProcessExecutionContext(context, context.ExecutionResult, true);
                    await _flowRepository.SaveContextHistory(_contextHistory);
                }

                else if (settings.StoreModel != FlowExecutionStoreModel.NoStoreTillStop && !settings.NoStorage)
                {
                    //await _storage.SaveProcessExecutionContext(context, context.ExecutionResult);
                    await _flowRepository.SaveContextHistory(_contextHistory);
                }

                return;
            }

            currentIteration++;

            if (currentIteration > MAX_LOOP_COUNT)
            {
                throw new FlowInfiniteExecutionException();
            }

            var task = flow.Tasks[i];
            context.CurrentTask = task.Name;
            context.CurrentTaskLine = i;

            try
            {
                switch (task.Type)
                {
                    case TaskDefTypes.Goto:
                        i = task.GotoIndex;
                        continue;

                    case TaskDefTypes.GotoIf:
                        if (task.Condition())
                        {
                            i = task.GotoIndex;
                        }

                        i++;
                        continue;

                    case TaskDefTypes.Label:
                        i++;
                        continue;

                    case TaskDefTypes.Begin:
                    case TaskDefTypes.Task:
                        await ExecuteTask(task, flow, context);
                        i++;
                        continue;

                    case TaskDefTypes.Wait:

                        if (task.Condition())
                        {
                            // stop execution and wait until the condition is met
                            context.ExecutionResult.ResultState = ResultStateEnum.Success;
                            context.ExecutionResult.FlowState = FlowStateEnum.Waiting;
                            context.ExecutionResult.IsWaitTask = true;
                        }
                        else
                        {
                            context.ExecutionResult.IsWaitTask = false;
                            i++;
                        }

                        continue;

                    case TaskDefTypes.WaitSignal:
                        await flow.WaitForSignalAsync(task.Signal);
                        i++;
                        continue;

                    case TaskDefTypes.WaitSignalTimeout:
                        await flow.WaitForSignalTimeoutAsync(task.Signal, task.Timeout.Value);
                        i++;
                        continue;

                    // ToDo: add Form task case when enable forms
                    //case TaskDefTypes.Form:
                    //    if (task.FormType != null)
                    //    {
                    //        // get an instance of the FormRulesCollection generic for this model, it must be registered in DI
                    //        var formInstance = _serviceProvider.GetService(task.FormType);
                    //        var formInstanceType = formInstance?.GetType();
                    //        var methodInfo = formInstanceType?.GetMethod("RootRule");

                    //        var formRuleInstance = methodInfo?.Invoke(formInstance, null);

                    //        var formRuleInstanceType = formRuleInstance?.GetType();
                    //        var formRuleMethodInfo = formRuleInstanceType?.GetMethod("Handle");

                    //        // Run the form rule
                    //        _ = formRuleMethodInfo?.Invoke(formRuleInstance, new object[] { context.Model }) is Task<bool> resultTask
                    //            && await resultTask;
                    //    }

                    //    if (task.Action is not null)
                    //    {
                    //        await ExecuteTask(task, flow, context);
                    //    }

                    //    if (context.ExecutionResult.FormState != FormTaskStateEnum.Submitted &&
                    //        context.ExecutionResult.FormState != FormTaskStateEnum.Rejected)
                    //    {
                    //        // stop execution and wait for Form submit
                    //        context.ExecutionResult.ResultState = ResultStateEnum.Success;
                    //        context.ExecutionResult.FlowState = FlowStateEnum.Stop;
                    //        context.ExecutionResult.FormId = task.FormType?.FullName ?? task.FormTypeName;
                    //        context.ExecutionResult.IsFormTask = true;
                    //        context.ExecutionResult.CallbackTaskId = task.CallbackTask;
                    //        context.ExecutionResult.PreloadTableData = task.PreloadTableData;
                    //    }
                    //    else
                    //    {
                    //        // form submitted - goto next task
                    //        context.ExecutionResult.ResultState = ResultStateEnum.Success;
                    //        context.ExecutionResult.FlowState = FlowStateEnum.Continue;
                    //        context.ExecutionResult.IsFormTask = false;
                    //        context.ExecutionResult.FormState = FormTaskStateEnum.Initialized;
                    //        i++;
                    //    }

                    //    continue;

                    case TaskDefTypes.End:
                        await ExecuteTask(task, flow, context);
                        context.ExecutionResult.FlowState = FlowStateEnum.Finished;
                        continue;

                    case TaskDefTypes.If:
                        if (task.Condition())
                        {
                            i++;
                        }
                        else
                        {
                            // goto Else/EndIf
                            i = task.GotoIndex;
                        }

                        continue;

                    case TaskDefTypes.Else:
                        // meet this operator only after passing all tasks inside if { ... } body, so use goto to EndIf
                        i = task.GotoIndex;
                        continue;

                    case TaskDefTypes.EndIf:
                        i++;
                        continue;
                }
            }
            catch (FlowStopException stop)
            {
                context.ExecutionResult.ResultState = ResultStateEnum.Success;
                context.ExecutionResult.FlowState = FlowStateEnum.Waiting;
                context.ExecutionResult.IsWaitTask = true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // flow run till the end, if End statement missed set it indirectly
        if (context.ExecutionResult.FlowState != FlowStateEnum.Finished)
        {
            context.ExecutionResult.ResultState = ResultStateEnum.Success;
            context.ExecutionResult.FlowState = FlowStateEnum.Finished;
            await AddContextToHistory(context);
            await _flowRepository.SaveContextHistory(_contextHistory);
        }
    }

    public async Task<FlowDefinitionDetails> GetFlowDefinitionDetails(FlowParams runParameters)
    {
        var result = new FlowDefinitionDetails();
        var flowType = runParameters.FlowType;
        //var refId = runParameters.RefId;
        //var parameters = runParameters.FlowParameters;
        //var noStorage = runParameters.NoStorageMode;
        //var context = runParameters.Context;

        // construct flow
        var flow = _services.GetService(flowType) as FlowBase;

        //var flowParameters = TypeHelper.GetConstructorParameters(_serviceProvider, flowType);
        //var flow = Activator.CreateInstance(flowType, flowParameters) as IFluentFlow;

        if (flow == null)
        {
            throw new FlowValidationException($"Flow of type '{flowType}' is not registered");
        }

        var flowBuilder = new FluentFlowBuilder(flow);
        flow.Define(flowBuilder);
        //flow.Parse();
        
        var stateNames = new HashSet<string>();
        var index = -1;
        TaskDetails task = null;
        //TaskDef prevTask = null;
        StateDef currentDef = null;
        StateDef prevDef = null;
        NextTask();

        while (index < flowBuilder.Tasks.Count)
        {
            ReadIfBlock();
            ReadTask();

            if (index == flowBuilder.Tasks.Count - 1)
            {
                break;
            }

            NextTask();
        }

        return result;

        void ReadTask()
        {
            //            if (task.Type == TaskDefTypes.Goto)
            //            {
            //	var newDef = GetTaskDef(task);
            //	result.States.Add(newDef);
            //	currentDef = newDef;

            //	if (prevDef != null)
            //	{
            //		var transition = GetTransitionDef(prevDef, currentDef);
            //		result.Transitions.Add(transition);
            //	}

            //                //var targetTask = flow.Tasks[task.GotoIndex];
            //                var labelDef = FindLabel(result, flow.Tasks[task.GotoIndex]);
            //	var gotoTransition = GetTransitionDef(currentDef, labelDef);
            //	result.Transitions.Add(gotoTransition);
            //}
            //            else
            if (task.Type != TaskDefTypes.If && task.Type != TaskDefTypes.EndIf)
            {
                var newDef = GetTaskDef(task);
                result.States.Add(newDef);
                currentDef = newDef;

                if (prevDef != null)
                {
                    var transition = GetTransitionDef(prevDef, currentDef);
                    result.Transitions.Add(transition);
                }
            }

            if (task.Type == TaskDefTypes.Goto)
            {
                var labelDef = FindLabel(result, flowBuilder.Tasks[task.GotoIndex]);
                var gotoTransition = GetTransitionDef(currentDef, labelDef);
                result.Transitions.Add(gotoTransition);
                prevDef = null;
                currentDef = null;
            }
            else if (task.Type == TaskDefTypes.GotoIf)
            {
                var labelDef = FindLabel(result, flowBuilder.Tasks[task.GotoIndex]);
                var gotoTransition = GetTransitionDef(currentDef, labelDef);
                result.Transitions.Add(gotoTransition);
                //prevDef = null;
                //currentDef = null;
            }
        }

        void NextTask()
        {
            index++;
            //prevTask = task;
            prevDef = currentDef;
            task = flowBuilder.Tasks[index];
        }

        void ReadIfBlock()
        {
            if (task.Type != TaskDefTypes.If)
            {
                return;
            }

            var startIfDef = GetTaskDef(task);
            result.States.Add(startIfDef);
            currentDef = startIfDef;

            if (prevDef != null)
            {
                var transition = GetTransitionDef(prevDef, currentDef);
                result.Transitions.Add(transition);
            }

            StateDef firstBranchEndDef = null;
            NextTask();

            while (task.Type != TaskDefTypes.EndIf)
            {
                if (task.Type == TaskDefTypes.Else)
                {
                    firstBranchEndDef = prevDef;
                    NextTask();
                    prevDef = startIfDef;
                }
                else
                {
                    ReadIfBlock();
                    ReadTask();
                    NextTask();
                }
            }

            //var startDef = GetTaskDef(startIf);
            var endDef = GetTaskDef(task);
            result.States.Add(endDef);
            currentDef = endDef;

            if (firstBranchEndDef != null)
            {
                // add first branch to endIf
                result.Transitions.Add(GetTransitionDef(firstBranchEndDef, endDef));
            }
            else
            {
                // add direct branch from if to endif
                result.Transitions.Add(GetTransitionDef(startIfDef, endDef));
            }

            // add second branch to endIf
            if (prevDef != null)
            {
                result.Transitions.Add(GetTransitionDef(prevDef, endDef));
            }

            //NextTask();
        }

        StateDef FindLabel(FlowDefinitionDetails details, TaskDetails task)
        {
            var result = details.States.FirstOrDefault(x => x.State == task.Name);

            if (result == null)
            {
                result = new StateDef
                {
                    State = $"{task.Index} : {task.Name}",
                    Type = task.Type.ToString(),
                };
            }

            return result;
        }

        StateDef GetTaskDef(TaskDetails task)
        {
            StateDef result;

            if (task.Type == TaskDefTypes.Goto || task.Type == TaskDefTypes.GotoIf)
            {
                result = new StateDef
                {
                    State = GetAvailableName($"{task.Type.ToString()} {task.Name}"),
                    //Caption = $"{task.Type.ToString()} {task.Name}",
                    Type = task.Type.ToString(),
                };
            }
            else if (task.Type == TaskDefTypes.If || task.Type == TaskDefTypes.EndIf)
            {
                result = new StateDef
                {
                    State = GetAvailableName($"{task.Index} : {task.Name}"),
                    Type = task.Type.ToString(),
                };
            }
            else
            {
                result = new StateDef
                {
                    State = GetAvailableName($"{task.Index} : {task.Name}"),
                    //Caption = task.Name,
                    Type = task.Type.ToString(),
                };
            }

            return result;
        }

        string GetAvailableName(string name)
        {
            if (stateNames.Contains(name))
            {
                int i = 2;

                while (stateNames.Contains($"{name}{i}"))
                {
                    i++;
                }

                string newName = $"{name}{i}";
                stateNames.Add(newName);
                return newName;
            }

            stateNames.Add(name);
            return name;
        }

        TransitionDef GetTransitionDef(StateDef from, StateDef to)
        {
            var result = new TransitionDef
            {
                FromState = from.State,
                ToState = to.State,
                Trigger = new TransitionTrigger { Text = "" },
            };

            return result;
        }
    }

}
