using MicroFlows.Application.Abstractions;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.UnitTesting;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MicroFlows.Tests.Activities;

public class ActivityExecutionTests : TestBase
{
    [Fact]
    public async Task CallActivity_WithTypedMethodSelector_ShouldExecuteSuccessfully()
    {
        // Arrange
        var engine = NewEngine();
        var flowParams = new FlowParams
        {
            FlowType = typeof(SimpleActivityFlow)
        };

        // Act
        var result = await engine.ExecuteFlow(typeof(SimpleActivityFlow), flowParams);
        var flowModel = await _repo.GetFlowModel(result.RefId);

        // Assert
        Assert.Equal(FlowStateEnum.Finished, result.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, result.ExecutionResult.ResultState);
        
        // Verify flow execution history
        // Debug: Check what's actually in the context history
        var contextCount = flowModel.ContextHistory.Count;
        var tasks = string.Join(", ", flowModel.ContextHistory.Select(h => h.CurrentTask));
        Assert.Equal("Begin:0, CallActivity:1, CallActivity:2, End:3", tasks);

        // Expecting: Begin + 2 CallActivity calls + End = 4 entries (return value CallActivity is commented out for now)
        Assert.True(contextCount >= 4, $"Expected at least 4 context entries, but got {contextCount}. Tasks: {tasks}");
        Assert.Equal("Begin:0", flowModel.ContextHistory[0].CurrentTask);
        
        // Verify the CallActivity calls were intercepted
        var callActivityEntries = flowModel.ContextHistory.Where(h => h.CurrentTask.Contains("CallActivity")).ToList();
        Assert.True(callActivityEntries.Count >= 2, $"Expected at least 2 CallActivity entries, but got {callActivityEntries.Count}. All tasks: {tasks}");
        
        // Check that activities were called (this will be implemented when CallActivity is added)
        // For now, this test will serve as a specification for the feature
    }
}

/// <summary>
/// Simple test flow that uses activities with typed method selectors
/// This is a specification for the future CallActivity implementation
/// </summary>
public class SimpleActivityFlow : FlowBase
{
    public bool EmailSent { get; set; }
    public string ProcessedEmail { get; set; } = string.Empty;
    public string ProcessedSubject { get; set; } = string.Empty;
    public int CalculationResult { get; set; }

    public async Task Flow()
    {
        // Test typed method selector with multiple parameters
        await CallActivity<EmailActivity>(
            a => a.SendEmail("test@example.com", "Welcome!", "Hello World!"));

        // Test typed method selector with return value
        // ToDo: return value interceptor is not implemented
        //var result = await CallActivity<CalculationActivity, int>(
        //    a => a.Add(100, 50));
        //CalculationResult = result;

        // Test typed method selector with single parameter
        await CallActivity<LogActivity>(
            a => a.LogMessage("Flow completed successfully"));
    }

    private async Task SimulateEmailActivity()
    {
        await Task.Delay(10);
        EmailSent = true;
        ProcessedEmail = "test@example.com";
        ProcessedSubject = "Welcome!";
    }

    private async Task SimulateCalculationActivity()
    {
        await Task.Delay(5);
        CalculationResult = 150; // 100 + 50
    }

    private async Task SimulateLogActivity()
    {
        await Task.Delay(5);
        // Simulate logging
    }
}

/// <summary>
/// Email activity for testing - Future implementation
/// </summary>
public class EmailActivity
{
    public async Task<bool> SendEmail(string email, string subject, string body)
    {
        // Simulate email sending
        await Task.Delay(10);
        return true;
    }

    public async Task<bool> SendWelcomeEmail(string email, string userName)
    {
        await Task.Delay(10);
        return await SendEmail(email, $"Welcome {userName}!", $"Hello {userName}, welcome to our service!");
    }

    public async Task<bool> Run(string email, string subject, string body)
    {
        // Default Run method
        return await SendEmail(email, subject, body);
    }
}

/// <summary>
/// Calculation activity for testing return values - Future implementation
/// </summary>
public class CalculationActivity
{
    public async Task<int> Add(int a, int b)
    {
        await Task.Delay(5);
        return a + b;
    }

    public async Task<decimal> Multiply(decimal a, decimal b)
    {
        await Task.Delay(5);
        return a * b;
    }

    public async Task<int> Run(int value)
    {
        // Default Run method
        await Task.Delay(5);
        return value * 2;
    }
}

/// <summary>
/// Logging activity for testing - Future implementation
/// </summary>
public class LogActivity
{
    public async Task LogMessage(string message)
    {
        await Task.Delay(5);
        // In real implementation, this would log to a logging framework
        Console.WriteLine($"Activity Log: {message}");
    }

    public async Task Run()
    {
        await LogMessage("Default log message");
    }
}