using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public class FlowBuilder : IFlowBuilder
{
    public List<TaskDetails> Tasks { get; internal set; } = [];

    public string CreateRefId()
    {
        throw new NotImplementedException();
    }

    public List<TaskDetails> Parse()
    {
        throw new NotImplementedException();
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
}
