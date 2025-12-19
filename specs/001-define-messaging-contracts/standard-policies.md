# Standard Messaging Policies

This document outlines the standard, default policies for handling transient errors, timeouts, and non-recoverable message failures (poison messages) that all Maliev microservices MUST adopt when configuring their MassTransit consumers.

## 1. Retry Policy (Transient Errors)

For transient errors like temporary network issues, database deadlocks, or short-lived service unavailability, an immediate, incremental retry policy MUST be used.

-   **Policy**: Retry 5 times.
-   **Interval**:
    -   1st retry: 1 second
    -   2nd retry: 2 seconds
    -   3rd retry: 4 seconds
    -   4th retry: 8 seconds
    -   5th retry: 16 seconds
-   **Rationale**: This provides a fast feedback loop for very short-lived issues while backing off exponentially to avoid overwhelming a struggling downstream service.

**MassTransit Configuration Example**:
```csharp
e.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
```

## 2. Timeout Policy

All external calls (e.g., HTTP requests) made within a message consumer should have a defined timeout.

-   **Policy**: A standard timeout of **30 seconds** should be applied to all outgoing requests.
-   **Rationale**: Prevents consumers from being blocked indefinitely by an unresponsive downstream service.

## 3. Poison Message & Dead-Lettering (Non-Recoverable Errors)

If a message fails all retry attempts, it is considered a "poison message" and must not be discarded. It must be moved to a centralized dead-letter queue (DLQ) for manual inspection and potential replay.

-   **Policy**: After the final retry fails, the message MUST be moved to an error queue.
-   **Error Queue Naming**: `[service-name]_error`
-   **Action**: The development team responsible for the consuming service MUST have monitoring and alerting set up on their error queue.
-   **Rationale**: Guarantees no message is lost. Provides a mechanism for developers to diagnose and fix issues with failing messages.

**MassTransit Configuration**: MassTransit handles this automatically by creating `_error` and `_skipped` queues when a message faults or is skipped. The key is to have monitoring on these resources.
