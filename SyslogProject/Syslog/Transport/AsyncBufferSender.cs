
namespace Syslog.Transport
{
    /// <summary>
    /// This class is primarily designed for the <see cref="SyslogClient"/>
    /// in conjunction with the SyslogTraceAdapter.
    ///
    /// So we have decoupled the handling of Trace write line, which should be fast,
    /// and sending data to remote host, which can be interrupted.
    /// 
    /// The <see cref="AsyncBufferSender"/> shut downs on error automatically.
    /// This is to keep the Trace pipeline intact for other listeners.
    ///
    /// In that case u have to reopen throught <see cref="Reset"/>.
    /// </summary>
    internal class AsyncBufferSender
    {
        // transport
        readonly IMessageSender sender;

        // message buffer
        // NOTE: Queue is not thread-safe
        //       so we have to lock pending for access
        readonly Queue<byte[]> pending = new();

        // lock pending access
        readonly object pendingLock = new();

        // lock close call
        readonly object closeLock = new();

        // detect SendDataAsync is running
        int counter = 0;

        enum OperationalState
        {
            Unitialized,
            Opened,
            Closed,
            Error
        }

        OperationalState Status = OperationalState.Unitialized;

        /// <summary>
        /// Number of pending messages to hold, before removing
        /// messages on overlow to keep the latest in the queue
        /// </summary>
        public int PendingLimit { get; set; } = 50;

        /// <summary>
        /// OnError Event
        /// </summary>
        public event EventHandler<Exception>? OnError;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="sender">MessageSender to use</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AsyncBufferSender(IMessageSender sender)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /// <summary>
        /// Ensures pending limit and adds a new payload to the buffer
        /// </summary>
        /// <param name="data"></param>
        private void AddPending(byte[] data)
        {
            lock (pendingLock)
            {
                while (pending.Count > PendingLimit)
                {
                     pending.Dequeue();
                }

                pending.Enqueue(data);
            }
        }

        /// <summary>
        /// Adds data to buffer and tries to send all pending messages
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            if (data.Length == 0)
            {
                return;
            }

            AddPending(data);

            // on these states we are unable to send any data
            switch (Status)
            {
                case OperationalState.Error:
                case OperationalState.Closed:
                    return;
            }

            // ensure only one caller is sending
            if (Interlocked.Increment(ref counter) == 1)
            {
                // dont wait
                SendDataAsync();
            }

            Interlocked.Decrement(ref counter);
        }

        /// <summary>
        /// Sends all pending messages, opens the transport if needed
        /// </summary>
        /// <returns></returns>
        private Task SendDataAsync()
        {
            // ensure only one caller is sending
            Interlocked.Increment(ref counter);

            return Task.Run(() =>
            {
                Exception? error = null;

                try
                {
                    Open();
                    Flush();
                }
                catch (Exception exception)
                {
                    error  = exception;
                    Status = OperationalState.Error;
                    sender.Close();
                }
                finally
                {
                    Interlocked.Decrement(ref counter);
                }

                // NOTE: invoke after Interlocked.Decrement
                if (error != null)
                {
                    // the good news: we are on a task so we did invoke "async"
                    OnError?.Invoke(this, error);
                }
            });
        }

        /// <summary>
        /// Open transport
        /// </summary>
        private void Open()
        {
            if (Status == OperationalState.Unitialized)
            {
                sender.Open();
                Status = OperationalState.Opened;
            }
        }

        /// <summary>
        /// Tries to open the transport to ensure network is up
        /// and error messages are written to console.
        /// </summary>
        public void TryOpen()
        {
            try
            {
                // ensure only one caller is sending
                if (Interlocked.Increment(ref counter) == 1)
                {
                    Open();
                }
            }
            catch (Exception error)
            {
                OnError?.Invoke(this, error);
            }
            finally
            {
                Interlocked.Decrement(ref counter);
            }
        }

        /// <summary>
        /// Sends pending messages from buffer to transport
        /// </summary>
        private void Flush()
        {
            while (Status == OperationalState.Opened)
            {
                byte[]? payload = null;

                // take message from buffer
                lock (pendingLock)
                {
                    if (pending.Count > 0)
                    {
                        payload = pending.Peek();
                    }
                    else
                    {
                        // no pending messages
                        return;
                    }
                }

                sender.Send(payload!);

                // remove message from buffer
                lock (pendingLock)
                {
                    if (pending.Count > 0)
                    {
                        pending.Dequeue();
                    }
                }
            }
        }

        /// <summary>
        /// Close the sender
        /// </summary>
        public void Close()
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(closeLock, 0, ref lockTaken);

                if (!lockTaken)
                {
                    return;
                }

                // ensure SendData cant call SendDataAsync
                Interlocked.Increment(ref counter);

                // wait while sending data or buffer is in use
                // -
                // we need a timeout, sometimes system resources
                // are unavailable (on shutdown or something else)
                // and we would hang ever here and then we would
                // block the close chunk
                // -
                // ok as i can see there must be a differnt in
                // net6.0 and net47 for Trace or DnsGetAddresses
                // ... have to review the source code (net47 problem)
                // -
                // TODO: find the net47 bug ... DnsGetAddresses?
                int timeout = 1000;
                SpinWait.SpinUntil(() => Interlocked.CompareExchange(ref counter, 1, 1) == 1, timeout);

                sender.Close();
                Status = OperationalState.Closed;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(closeLock);
                    Interlocked.Decrement(ref counter);
                }
            }
        }

        /// <summary>
        /// Resets the operational state
        /// </summary>
        public void Reset()
        {
            Close();
            Status = OperationalState.Unitialized;
        }
    }
}
