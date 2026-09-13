using Core.Interfaces.Services;
using System.Threading.Channels;

namespace Infrastructure.Services
{
    /// <summary>
    /// In-memory fire-and-forget queue. A request handler enqueues work and returns
    /// immediately; a single BackgroundService (QueuedHostedService) drains the queue
    /// in the same process. Nothing here survives a process restart/sleep. It's for
    /// "kick off a quick reconciliation whenever someone happens to log in", not a
    /// durable job scheduler.
    /// </summary>
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
            Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
                new UnboundedChannelOptions { SingleReader = true });

        public void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            _queue.Writer.TryWrite(workItem);
        }

        public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
