using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using System;

namespace Shared.Infrastructure;

/// <summary>
/// Extension methods for configuring circuit breaker patterns
/// </summary>
public static class CircuitBreakerExtensions
{
    /// <summary>
    /// Adds circuit breaker policy to HttpClient
    /// </summary>
    public static IHttpClientBuilder AddCircuitBreaker(this IHttpClientBuilder builder)
    {
        return builder.AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    /// <summary>
    /// Gets a circuit breaker policy with retry logic
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    // Log circuit breaker opened
                    Console.WriteLine($"Circuit breaker opened. Duration: {duration}");
                },
                onReset: () =>
                {
                    // Log circuit breaker reset
                    Console.WriteLine("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    // Log circuit breaker half-open
                    Console.WriteLine("Circuit breaker half-open");
                });
    }

    /// <summary>
    /// Gets a retry policy with exponential backoff
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds} seconds");
                });
    }

    /// <summary>
    /// Adds retry policy to HttpClient
    /// </summary>
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
    {
        return builder.AddPolicyHandler(GetRetryPolicy());
    }
}

