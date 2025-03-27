using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Models;

public class FlowDefinitionDetails
{
    public List<StateDef> States { get; internal set; } = new();
    public List<TransitionDef> Transitions { get; internal set; } = new();
    //public List<FormDef> Forms { get; internal set; }
    //public string CurrentState { get; internal set; }
    //public List<TransitionDef> CurrentStateTransitions { get; internal set; }
    //public Dictionary<TransitionDef, StateFlowTransitionSelector> CurrentStateSelectors { get; internal set; } = new Dictionary<TransitionDef, StateFlowTransitionSelector>();
}

public class StateDef
{
    public string State { get; set; }
    public string Type { get; set; }
    public string Caption { get; set; }
    public bool IsEnd { get; internal set; }
    public Func<Task> OnBeginAsync { get; internal set; }
}

public class TransitionDef
{
    public string FormType { get; set; }
    public string FromState { get; set; }
    public string ToState { get; set; }
    public TransitionTrigger Trigger { get; set; }
    public Func<TransitionTrigger> TriggerFunction { get; set; }
    public Action OnChanging { get; set; }
    public Func<Task> OnChangingAsync { get; set; }

    public TransitionTrigger GetTrigger()
    {
        return Trigger ?? TriggerFunction();
    }

    //public bool IsButtonTrigger()
    //{
    //    var trigger = GetTrigger();
    //    return trigger is ButtonTransitionTrigger;
    //}

    //public bool IsUserActionTrigger()
    //{
    //    var trigger = GetTrigger();
    //    return trigger is UserActionTransitionTrigger;
    //}
}

public class TransitionTrigger
{
    public string Text { get; internal set; }
    public string CommandText { get; internal set; }
    public bool Proceed { get; set; }

    public TransitionTrigger()
    { }

    //public TransitionTrigger(state state)
    //{ }

    public virtual void CheckTrigger(FlowContext context)
    {
    }

    public virtual async Task CheckTriggerAsync(FlowContext context)
    {
    }

    public virtual bool IsTriggerAsync()
    {
        return false;
    }

    //public virtual StateFlowTransitionSelector GetSelector()
    //{
    //    return null;
    //}
}
