using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicroFlows.Application.Exceptions;
using JsonPathToModel;
using System.Linq;
using MicroFlows.Application;
using Microsoft.Extensions.DependencyInjection;

namespace MicroFlows;

/// <summary>
/// Base class for Flows
/// All properties and fields except privates will be tracked as Model
/// </summary>
public abstract partial class FlowBase
{
    //[JsonIgnore]
    //public bool IsFluentFlow { get;private set; } = true;

    //[JsonIgnore]
    //public List<TaskDetails> Tasks => throw new NotImplementedException();

    /// <summary>
    /// Fluent flow definition
    /// You should override this method to use Fluent Flow Notation
    /// </summary>
    public virtual void Define(IFlowBuilder builder)
    {
        //IsFluentFlow = false;
    }

    //public string CreateRefId()
    //{
    //    throw new NotImplementedException();
    //}

    //public List<TaskDetails> Parse()
    //{
    //    throw new NotImplementedException();
    //}

    //public void SetFirstPass(bool firstPass)
    //{
    //    throw new NotImplementedException();
    //}

    //public void SetFlowRefId(string refId)
    //{
    //    throw new NotImplementedException();
    //}

    //public void UpdateCurrentContext(string statusMessage, string assignedUser, string adminUser, string assignedTeam)
    //{
    //    throw new NotImplementedException();
    //}
}
