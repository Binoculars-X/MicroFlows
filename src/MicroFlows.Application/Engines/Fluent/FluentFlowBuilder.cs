using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroFlows;

public class FluentFlowBuilder : IFlowBuilder
{
    protected readonly FlowBase _flow;

    public FluentFlowBuilder(FlowBase flow)
    {
        _flow = flow;
    }

    public List<TaskDetails> Tasks { get; internal set; } = [];

    public string CreateRefId()
    {
        throw new NotImplementedException();
    }

    public List<TaskDetails> Parse()
    {
        // Clear Tasks list
        Tasks = [];

        // 1st pass - create task list
        _flow.Define(this);
        _flow.Tasks = Tasks;

        // 2nd pass - goto indexes
        var labels = Tasks.Select((s, i) => new { s, i }).Where(t => t.s.Type == TaskDefTypes.Label).ToDictionary(x => x.s.Name, x => x.i);
        Tasks.Where(t => t.Type == TaskDefTypes.Goto || t.Type == TaskDefTypes.GotoIf).ToList().ForEach(t => t.GotoIndex = labels[t.Name]);

        // 3rd pass - if-else-endif indexes
        Stack<StackType> stack = new Stack<StackType>();

        for (int i = 0; i < Tasks.Count; i++)
        {
            var task = Tasks[i];

            if (task.Type == TaskDefTypes.If)
            {
                stack.Push(new StackType { If = true, Index = i });
            }
            else if (task.Type == TaskDefTypes.Else)
            {
                var st = stack.Pop();

                if (st.If == false)
                {
                    throw new Exception("Else must be used after If only");
                }

                int ifIndex = st.Index;
                Tasks[ifIndex].GotoIndex = i + 1;
                stack.Push(new StackType { If = false, Index = i });
            }
            else if (task.Type == TaskDefTypes.EndIf)
            {
                var st = stack.Pop();

                if (st.If == true)
                {
                    // Else is missed - goto directly after EndIf
                    int ifIndex = st.Index;
                    Tasks[ifIndex].GotoIndex = i + 1;
                }
                else
                {
                    // set Else goto index
                    int elseIndex = st.Index;
                    Tasks[elseIndex].GotoIndex = i + 1;
                }
            }
        }

        for (int i = 0; i < Tasks.Count; i++)
        {
            Tasks[i].Index = i;
        }

        return Tasks;
    }

    public void SetFirstPass(bool firstPass)
    {
        throw new NotImplementedException();
    }

    public void SetFlowRefId(string refId)
    {
        throw new NotImplementedException();
    }

    public void UpdateCurrentContext(string statusMessage, string assignedUser, string adminUser, string assignedTeam)
    {
        throw new NotImplementedException();
    }

    private class StackType
    {
        public bool If;
        public int Index;
    }
}
