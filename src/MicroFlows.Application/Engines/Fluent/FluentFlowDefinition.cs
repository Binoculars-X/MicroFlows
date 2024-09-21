using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Domain.Models;

namespace MicroFlows;

public static class FluentFlowDefinition
{
    public static F Begin<F>(this F flow) where F : class, IFlowBuilder { RegisterStart(flow.Tasks, null); return flow; }
    public static F Begin<F>(this F flow, Func<Task> action) where F : class, IFlowBuilder { RegisterStart(flow.Tasks, action); return flow; }
    public static F End<F>(this F flow, Func<Task> action) where F : class, IFlowBuilder { RegisterFinish(flow.Tasks, action); return flow; }
    public static F End<F>(this F flow) where F : class, IFlowBuilder { RegisterFinish(flow.Tasks, null); return flow; }
    public static F Next<F>(this F flow, Func<Task> action) where F : class, IFlowBuilder { RegisterTask(flow.Tasks, action.Method.Name, action); return flow; }
    public static F Next<F>(this F flow, Action action) where F : class, IFlowBuilder { RegisterTask(flow.Tasks, action.Method.Name, action); return flow; }
    //public static F NextForm<F>(this F flow, Type formType, Func<Task> action) where F : class, IFluentFlow { RegisterFormTask(flow.Tasks, formType.Name, formType, action); return flow; }

    //public static F NextForm<F>(this F flow, Type formType) where F : class, IFluentFlow { RegisterFormTask(flow.Tasks, formType.Name, formType); return flow; }
    //public static F NextForm<F>(this F flow, string formName) where F : class, IFluentFlow { RegisterFormTask(flow.Tasks, formName, formName); return flow; }

    //public static void ListForm<M>(this IFluentFlow flow, Type formType, Func<QueryOptions, Task<M>> callBack) where M : class, IFlowModel
    //{
    //    RegisterListFormTask(flow.Tasks, formType.Name, formType, callBack);
    //}

    //public static void ListForm<M>(this IFluentFlow flow, Type formType, Func<QueryOptions, Task<M>> callBack, bool preloadTableData) where M : class, IFlowModel
    //{
    //    RegisterListFormTask(flow.Tasks, formType.Name, formType, callBack, preloadTableData);
    //}

    public static F Goto<F>(this F flow, string label) where F : class, IFlowBuilder { RegisterGoto(flow.Tasks, label); return flow; }
    public static F Label<F>(this F flow, string label) where F : class, IFlowBuilder { RegisterLabel(flow.Tasks, label); return flow; }
    public static F GotoIf<F>(this F flow, string label, Func<bool> condition) where F : class, IFlowBuilder { RegisterGotoIf(flow.Tasks, label, condition); return flow; }
    public static F If<F>(this F flow, Func<bool> condition) where F : class, IFlowBuilder { RegisterIf(flow.Tasks, condition); return flow; }
    public static F Else<F>(this F flow) where F : class, IFlowBuilder { RegisterTask(flow.Tasks, TaskDefTypes.Else, "Else", null); return flow; }
    public static F EndIf<F>(this F flow) where F : class, IFlowBuilder { RegisterTask(flow.Tasks, TaskDefTypes.EndIf, "EndIf", null); return flow; }
    public static F Wait<F>(this F flow, Func<bool> condition) where F : class, IFlowBuilder { RegisterWait(flow.Tasks, condition); return flow; }

    private static void RegisterTask(List<TaskDetails> tasks, TaskDefTypes type, string name, Func<Task> action)
    {
        tasks.Add(new TaskDetails { Action = action, Name = name, Type = type });
    }

    private static void RegisterWait(List<TaskDetails> tasks, Func<bool> condition)
    {
        tasks.Add(new TaskDetails { Name = "Wait", Type = TaskDefTypes.Wait, Condition = condition });
    }

    private static void RegisterIf(List<TaskDetails> tasks, Func<bool> condition)
    {
        tasks.Add(new TaskDetails { Name = "If", Type = TaskDefTypes.If, Condition = condition });
    }

    private static void RegisterGoto(List<TaskDetails> tasks, string name)
    {
        // the second pass will update GotoIndex
        tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Goto, GotoIndex = -1 });
    }

    private static void RegisterGotoIf(List<TaskDetails> tasks, string name, Func<bool> condition)
    {
        // the second pass will update GotoIndex
        tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.GotoIf, GotoIndex = -1, Condition = condition });
    }

    private static void RegisterStart(List<TaskDetails> tasks, Func<Task> action)
    {
        RegisterTask(tasks, TaskDefTypes.Begin, "Start", action);
    }

    private static void RegisterFinish(List<TaskDetails> tasks, Func<Task> action)
    {
        RegisterTask(tasks, TaskDefTypes.End, "Finish", action);
    }

    //private static void RegisterFormTask(List<TaskDetails> tasks, string name, Type formType)
    //{
    //    tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Form, FormType = formType });
    //}
    //private static void RegisterFormTask(List<TaskDetails> tasks, string name, string formTypeName)
    //{
    //    tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Form, FormTypeName = formTypeName });
    //}

    //private static void RegisterListFormTask<M>(List<TaskDetails> tasks, string name, Type formType, Func<QueryOptions, Task<M>> callBack, bool preloadTableData = false) where M : class, IFlowModel
    //{
    //    tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Form, FormType = formType, CallbackTask = callBack.Method.Name, PreloadTableData = preloadTableData });
    //}
    //private static void RegisterListFormTask<M>(List<TaskDetails> tasks, string name, Type formType, Func<dynamic, Task<M>> callBack) where M : class, IFlowModel
    //{
    //    tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Form, FormType = formType, CallbackTask = callBack.Method.Name });
    //}

    private static void RegisterTask(List<TaskDetails> tasks, string name, Func<Task> action)
    {
        RegisterTask(tasks, TaskDefTypes.Task, name, action);
    }

    private static void RegisterTask(List<TaskDetails> tasks, string name, Action action)
    {
        tasks.Add(new TaskDetails { NonAsyncAction = action, Name = name, Type = TaskDefTypes.Task });
    }

    //private static void RegisterFormTask(List<TaskDetails> tasks, string name, Type formType, Func<Task> action)
    //{
    //    tasks.Add(new TaskDetails { Name = name, Type = TaskDefTypes.Form, FormType = formType, Action = action });
    //}

    private static void RegisterLabel(List<TaskDetails> tasks, string name)
    {
        RegisterTask(tasks, TaskDefTypes.Label, name, null);
    }
}

