using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;
public interface IFlowRunEngine
{
    Task ExecuteFlow(Type flowType);
    Task ExecuteFlow(string flowTypeName);
    Task ResumeFlow(string flowId);
}
