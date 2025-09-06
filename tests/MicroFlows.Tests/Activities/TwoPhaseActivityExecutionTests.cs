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

public class TwoPhaseActivityExecutionTests : TestBase
{
    [Fact]
    public async Task CallActivity_WithSignalWaiting_ShouldExecuteInTwoPasses()
    {
        // Arrange - First pass: execute until signal wait
        var engine = NewEngine();
        var flowParams = new FlowParams
        {
            FlowType = typeof(TwoPhaseActivityFlow),
            ExternalId = "order-123"
        };

        // Act - First pass: execute flow until it waits for signal
        var firstResult = await engine.ExecuteFlow(typeof(TwoPhaseActivityFlow), flowParams);
        var firstFlowModel = await _repo.GetFlowModel(firstResult.RefId);

        // Assert - First pass: flow should be waiting for signal, first activity executed
        Assert.Equal(FlowStateEnum.Waiting, firstResult.ExecutionResult.FlowState);
        Assert.Equal(ResultStateEnum.Success, firstResult.ExecutionResult.ResultState);
        
        // Verify first activity was executed
        var firstTasks = string.Join(", ", firstFlowModel.ContextHistory.Select(h => h.CurrentTask));
        var firstCallActivityEntries = firstFlowModel.ContextHistory.Where(h => h.CurrentTask.Contains("CallActivity")).ToList();
        Assert.True(firstCallActivityEntries.Count >= 1, $"Expected at least 1 CallActivity entry in first pass, but got {firstCallActivityEntries.Count}. Tasks: {firstTasks}");

        // Arrange - Second pass: send signal and continue execution
        var secondEngine = NewEngine();
        
        // Act - Second pass: send signal to continue flow execution
        await secondEngine.SendSignal(typeof(TwoPhaseActivityFlow), "payment-received", flowParams);
        var secondFlowModel = await _repo.GetFlowModel(firstResult.RefId);

        // Assert - Second pass: flow should be finished, second activity executed
        Assert.Equal(FlowStateEnum.Finished, secondFlowModel.State);
        
        // Verify both activities were executed across the two passes
        var allTasks = string.Join(", ", secondFlowModel.ContextHistory.Select(h => h.CurrentTask));
        var allCallActivityEntries = secondFlowModel.ContextHistory.Where(h => h.CurrentTask.Contains("CallActivity")).ToList();
        Assert.True(allCallActivityEntries.Count >= 2, $"Expected at least 2 CallActivity entries total, but got {allCallActivityEntries.Count}. Tasks: {allTasks}");
        
        // Verify signal waiting was recorded
        var signalEntries = secondFlowModel.ContextHistory.Where(h => h.CurrentTask.Contains("WaitForSignal")).ToList();
        Assert.True(signalEntries.Count >= 1, $"Expected at least 1 WaitForSignal entry, but got {signalEntries.Count}. Tasks: {allTasks}");
    }
}

/// <summary>
/// Two-phase flow that executes activities before and after signal waiting
/// This demonstrates complex flow execution with activities and signals
/// </summary>
public class TwoPhaseActivityFlow : FlowBase
{
    public bool OrderProcessed { get; set; }
    public bool PaymentProcessed { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }

    public async Task Flow()
    {
        // Phase 1: Process order before payment
        await CallActivity<OrderProcessingActivity>(
            a => a.ProcessOrder("order-123", 199.99m));
        
        OrderProcessed = true;
        OrderStatus = "Processed";
        OrderAmount = 199.99m;

        // Wait for payment signal
        await WaitForSignalAsync("payment-received");

        // Phase 2: Process payment after signal received
        await CallActivity<PaymentProcessingActivity>(
            a => a.ProcessPayment("order-123", 199.99m));
        
        PaymentProcessed = true;
        OrderStatus = "Completed";
    }
}

/// <summary>
/// Order processing activity for the first phase
/// </summary>
public class OrderProcessingActivity
{
    public async Task ProcessOrder(string orderId, decimal amount)
    {
        // Simulate order processing
        await Task.Delay(10);
        Console.WriteLine($"Activity Log: Order {orderId} processed for ${amount}");
    }

    public async Task ValidateOrder(string orderId)
    {
        await Task.Delay(5);
        Console.WriteLine($"Activity Log: Order {orderId} validated");
    }

    public async Task Run(string orderId)
    {
        // Default Run method
        await ProcessOrder(orderId, 0);
    }
}

/// <summary>
/// Payment processing activity for the second phase
/// </summary>
public class PaymentProcessingActivity
{
    public async Task ProcessPayment(string orderId, decimal amount)
    {
        // Simulate payment processing
        await Task.Delay(15);
        Console.WriteLine($"Activity Log: Payment processed for order {orderId}, amount: ${amount}");
    }

    public async Task RefundPayment(string orderId, decimal amount)
    {
        await Task.Delay(10);
        Console.WriteLine($"Activity Log: Payment refunded for order {orderId}, amount: ${amount}");
    }

    public async Task Run(string orderId)
    {
        // Default Run method
        await ProcessPayment(orderId, 0);
    }
}