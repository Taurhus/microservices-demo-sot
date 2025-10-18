using System;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests.TestHelpers
{
    // Collection fixture that waits for the microservices' HTTP ports to be reachable before any tests run.
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<TestCollectionFixture>
    {
    }

    public class TestCollectionFixture : IDisposable
    {
        private readonly TimeSpan _timeout;

        public TestCollectionFixture()
        {
            // Read timeout from environment variable (seconds) with a sensible default.
            var timeoutSeconds = 30;
            var envTimeout = Environment.GetEnvironmentVariable("SERVICE_WAIT_TIMEOUT_SECONDS");
            if (!string.IsNullOrWhiteSpace(envTimeout) && int.TryParse(envTimeout, out var parsed))
            {
                timeoutSeconds = parsed;
            }

            _timeout = TimeSpan.FromSeconds(timeoutSeconds);

            // Run wait synchronously so xUnit waits for fixture completion before discovering/running tests.
            Task.Run(async () => await WaitForServices()).GetAwaiter().GetResult();
        }

        private async Task WaitForServices()
        {
            // Default services (host:port) used by the tests. This can be overridden by the SERVICE_PORTS env var.
            var defaults = new string[]
            {
                "127.0.0.1:5001", // Player
                "127.0.0.1:5002", // Ship
                "127.0.0.1:5003", // Quest
                "127.0.0.1:5004", // Faction
                "127.0.0.1:5005", // Event
                "127.0.0.1:5006", // Item
                "127.0.0.1:5007", // Location
                "127.0.0.1:5008", // Shop
            };

            var env = Environment.GetEnvironmentVariable("SERVICE_PORTS");
            var entries = string.IsNullOrWhiteSpace(env) ? defaults : env.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                var parts = entry.Trim().Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
                {
                    throw new InvalidOperationException($"Invalid SERVICE_PORTS entry: '{entry}'. Use host:port comma-separated list.");
                }

                var host = parts[0];
                var ok = await TestStartupWait.WaitForTcpAsync(host, port, _timeout);
                if (!ok)
                {
                    // If a service doesn't come up in time, throw to fail fast with a clear message.
                    throw new InvalidOperationException($"Service at {host}:{port} not available after {_timeout.TotalSeconds} seconds.");
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
