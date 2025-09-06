# Activities Design

## Key Features:

1. **Non-Deterministic Code**: Unlike Workflows (which must be deterministic), Activities can contain non-deterministic code like:
   - Network calls
   - File I/O operations
   - Random number generation
   - Current time access

2. **Idempotent by Design**: Temporal strongly recommends Activities be idempotent, meaning:
   - Executing the same Activity multiple times produces the same result
   - Safe to retry without unwanted side effects
   - Uses idempotency keys to handle duplicate requests

3. **Automatic Retry & Recovery**: Activities have built-in:
   - Automatic retries on failure
   - Configurable retry policies
   - Timeout management
   - Heartbeat mechanisms for long-running tasks

4. **Activity Lifecycle & Timeouts**: Activities have several timeout configurations:
   - **Schedule-To-Start Timeout**: Max time from when task is queued to when worker starts it
   - **Start-To-Close Timeout**: Max time for a single activity execution attempt
   - **Schedule-To-Close Timeout**: Max time for entire activity execution (including retries)
   - **Heartbeat Timeout**: Max time between heartbeats for long-running activities

5. **Activity Heartbeats**: For long-running Activities, Temporal provides heartbeat mechanisms:
   - Regular "ping" from worker to Temporal service
   - Indicates progress and worker health
   - Can include progress payload data
   - Enables cancellation delivery
   - Prevents unnecessary timeouts

## Current MicroFlows Implementation Status

**Note**: Need to investigate if MicroFlows currently has any activity-related features or if this is a new capability being added in this branch (feature/6-support-activities).

## Proposed MicroFlows Activities Implementation

### CallActivity Generic Method

We introduce a new generic method `CallActivity<T>()` where `T` is the Activity implementation class:

```csharp
// Generic CallActivity method
public async Task<TResult> CallActivity<TActivity, TResult>(
    Expression<Func<TActivity, Task<TResult>>> methodSelector,
    ActivityOptions? options = null)
    where TActivity : class

// Overload for void methods
public async Task CallActivity<TActivity>(
    Expression<Func<TActivity, Task>> methodSelector,
    ActivityOptions? options = null)
    where TActivity : class

// Overload with method name string
public async Task<TResult> CallActivity<TActivity, TResult>(
    string methodName,
    object[] parameters,
    ActivityOptions? options = null)
    where TActivity : class

// Default Run method execution
public async Task<TResult> CallActivity<TActivity, TResult>(
    object[] parameters = null,
    ActivityOptions? options = null)
    where TActivity : class
```

### Activity Class Design

Activity classes can have multiple methods. If no specific method is provided, the `Run` method should be implemented:

```csharp
public class EmailActivity
{
    // Default method - called when no method specified
    public async Task<bool> Run(string email, string subject, string body)
    {
        return await SendEmail(email, subject, body);
    }
    
    // Specific methods for different operations
    public async Task<bool> SendWelcomeEmail(string email, string userName)
    {
        // Implementation
    }
    
    public async Task<bool> SendNotificationEmail(string email, string message)
    {
        // Implementation
    }
}

// Usage examples:
public class MyWorkflow : FlowBase
{
    public async Task Flow()
    {
        // Call specific method using expression
        await CallActivity<EmailActivity>(
            a => a.SendWelcomeEmail(userEmail, userName),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        
        // Call specific method using method name
        await CallActivity<EmailActivity>("SendNotificationEmail", 
            new object[] { userEmail, notificationMessage });
        
        // Call default Run method
        await CallActivity<EmailActivity>(
            new object[] { userEmail, "Subject", "Body" });
    }
}
```

### Method Resolution Priority

1. **Expression-based method selection** (strongly typed, compile-time safe)
2. **Method name string** (runtime resolution, flexible)
3. **Default Run method** (fallback when no method specified)

### AddSignalActivity Method

The `AddSignalActivity` method allows binding received signals with activity execution:

```csharp
// Generic AddSignalActivity method
public void AddSignalActivity<TActivity>(
    string signalName,
    Expression<Func<TActivity, Task>> methodSelector)
    where TActivity : class

// Overload with method name
public void AddSignalActivity<TActivity>(
    string signalName,
    string methodName = "Run")
    where TActivity : class
```

### Signal-Activity Integration Features

1. **Automatic Payload Injection**: If the signal has a payload, it will be automatically supplied as a parameter to the activity method
2. **Model Parameter Support**: For typed flows (`FlowBase<TModel>`), the model can be supplied as a parameter to the activity
3. **Flexible Parameter Mapping**: The framework automatically maps signal payload and model to activity method parameters

```csharp
public class OrderProcessingActivity
{
    // Activity method that receives signal payload and model
    public async Task ProcessOrderSignal(OrderPayload payload, OrderModel model)
    {
        // Process the order using both signal data and current workflow model
        await ProcessOrder(payload.OrderId, model.CustomerId);
    }
    
    // Simple activity method for signals without payload
    public async Task Run()
    {
        // Default signal processing
    }
}

// Usage in workflow:
public class OrderWorkflow : FlowBase<OrderModel>
{
    public async Task Flow()
    {
        // Bind signal to activity with automatic payload/model injection
        AddSignalActivity<OrderProcessingActivity>(
            "order-received", 
            a => a.ProcessOrderSignal(default, default)); // Parameters auto-injected
        
        // Or using method name
        AddSignalActivity<OrderProcessingActivity>("payment-confirmed", "ProcessPayment");
        
        await WaitForSignalAsync("order-received");
        // When signal arrives, OrderProcessingActivity.ProcessOrderSignal will be executed
        // with signal payload and current model automatically injected
    }
}
```

**Reference**: See `FlowEngineActivitiesTests` for detailed examples of signal-activity integration patterns.