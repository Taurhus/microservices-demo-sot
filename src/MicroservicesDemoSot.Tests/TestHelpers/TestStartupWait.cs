using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MicroservicesDemoSot.Tests.TestHelpers
{
    internal static class TestStartupWait
    {
        /// <summary>
        /// Waits for a TCP host:port to accept connections until timeout.
        /// Returns true if the port becomes available within the timeout.
        /// </summary>
        public static async Task<bool> WaitForTcpAsync(string host, int port, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                try
                {
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var connectTask = socket.ConnectAsync(host, port);
                    var completed = await Task.WhenAny(connectTask, Task.Delay(500));
                    if (completed == connectTask && socket.Connected)
                    {
                        socket.Close();
                        return true;
                    }
                }
                catch
                {
                    // ignore and retry
                }

                await Task.Delay(250);
            }

            return false;
        }
    }
}
