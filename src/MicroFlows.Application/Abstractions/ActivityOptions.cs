using System;

namespace MicroFlows.Application.Abstractions;

/// <summary>
/// Options for activity execution
/// </summary>
public class ActivityOptions
{
    /// <summary>
    /// Maximum time for activity execution from start to completion
    /// </summary>
    public TimeSpan? StartToCloseTimeout { get; set; }
    
    /// <summary>
    /// Maximum time for activity execution including retries
    /// </summary>
    public TimeSpan? ScheduleToCloseTimeout { get; set; }
    
    /// <summary>
    /// Retry policy for the activity
    /// </summary>
    public ActivityRetryPolicy? RetryPolicy { get; set; }
    
    /// <summary>
    /// Heartbeat timeout for long-running activities
    /// </summary>
    public TimeSpan? HeartbeatTimeout { get; set; }
}

/// <summary>
/// Retry policy for activities
/// </summary>
public class ActivityRetryPolicy
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaximumAttempts { get; set; } = 3;
    
    /// <summary>
    /// Initial retry interval
    /// </summary>
    public TimeSpan InitialInterval { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Maximum retry interval
    /// </summary>
    public TimeSpan MaximumInterval { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Backoff coefficient for exponential backoff
    /// </summary>
    public double BackoffCoefficient { get; set; } = 2.0;
}