using MicroFlows.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public interface IFlowBuilder //: IFlow
{
    //void Define();
    List<TaskDetails> Parse();
    //IFlowModel GetModel();
    //void SetModel(IFlowModel model);
    //void SetParams(FlowParamsGeneric p);
    //IFlowContext CreateContext();
    List<TaskDetails> Tasks { get; }
    //void SetFlowRefId(string refId);
    //void SetFirstPass(bool firstPass);
    //string CreateRefId();
    //void SetFlowContext(IFlowContext flowContext);
    void UpdateCurrentContext(string statusMessage, string assignedUser, string adminUser, string assignedTeam);
}

//public interface IFluentFlow<TModel> : IFluentFlow where TModel : class, new()
//{
//    TModel Model { get; set; }
//}