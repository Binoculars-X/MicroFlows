using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;
public class FlowContext
{
    public string RefId { get; set; } = null!;
    public string FlowId { get; set; } = null!;
    public IFlow Model { get; set; } = null!;
    public FlowParams Params { get; set; } = null!;
    public string CurrentTask { get; set; }
    public int CurrentTaskLine { get; set; }
    public List<string> CallStack { get; set; }
    public TaskExecutionResult ExecutionResult { get; set; }
}
