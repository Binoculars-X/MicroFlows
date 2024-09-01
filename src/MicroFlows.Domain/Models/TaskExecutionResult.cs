using MicroFlows.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;
public class TaskExecutionResult
{
    public ResultStateEnum ResultState { get; set; }
    public FlowStateEnum FlowState { get; set; }
    //public FormTaskStateEnum FormState { get; set; }
    public string FormLastAction { get; set; }
    public bool IsFormTask { get; set; }
    public string FormId { get; set; }
    public string CallbackTaskId { get; set; }
    public string NextStep { get; set; }
    public string ExceptionMessage { get; set; }
    //public List<TaskExecutionValidationResult> TaskExecutionValidationIssues { get; set; } = new List<TaskExecutionValidationResult>();
    public string ExceptionStackTrace { get; set; }
    public string ExceptionType { get; set; }
    public Exception ExecutionException { get; set; }
}
