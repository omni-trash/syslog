using System.Net.Sockets;

namespace SampleServer
{
    public class UdpServer
    {
        /// <summary>
        /// OnError Event
        /// </summary>
        public event EventHandler<Exception>? OnError;

        /// <summary>
        /// OnResult Event
        /// </summary>
        public event EventHandler<UdpReceiveResult>? OnResult;

        // local port to use
        readonly int localPort;

        public UdpServer(int port)
        {
            this.localPort = port;
        }

        /// <summary>
        /// Run and wait
        /// </summary>
        /// <param name="quit"></param>
        public void Run(WaitHandle quit)
        {
            try
            {
                using UdpClient client = new(localPort);
                client.EnableBroadcast = true;

                CancellationTokenSource cancellation = new();

                var receiver = UdpReceiverTask(client, cancellation.Token);

                quit.WaitOne();
                cancellation.Cancel();
                receiver.Wait();
            }
            catch (Exception error)
            {
                OnError?.Invoke(this, error);
            }
        }

        /// <summary>
        /// Listen for UDP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UdpReceiverTask(UdpClient client, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            cancellationToken.Register(() =>
            {
                tcs.SetResult(true);
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // net47 unavailable
                    // UdpReceiveResult result = await client.ReceiveAsync(cancellationToken);

                    UdpReceiveResult? result = null;

                    Task task = Task.Run(async () =>
                    {
                        result = await client.ReceiveAsync();
                    });

                    if (task != await Task.WhenAny(task, tcs.Task))
                    {
                        // Terminate
                        client.Close();
                        throw new OperationCanceledException();
                    }

                    if (result.HasValue)
                    {
                        OnResult?.Invoke(this, result.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception error)
                {
                    OnError?.Invoke(this, error);
                }
            };
        }
    }
}
