using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Application.Abstractions;


public interface IFlowEnvironment
{
    public bool TimeoutOccurred { get; }
}
    
public class FlowEnvironment : IFlowEnvironment
{
    public bool TimeoutOccurred { get; set; }
}
