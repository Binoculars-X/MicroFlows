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

namespace MicroFlows.Application.Engines.Interceptors;
    
// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
internal partial class FlowEngine
{
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

        var flowBuilder = new FlowBuilder();
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
