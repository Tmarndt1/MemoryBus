namespace MemBus
{
    /// <summary>
    /// Base type for subscriptions that can unregister themselves when disposed.
    /// </summary>
    public abstract class Subscriber : IDisposable
    {
        private readonly object _disposeLock = new();
        private readonly List<Action> _disposeActions = new();

        /// <summary>
        /// Unique identifier used to unsubscribe this subscriber.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets whether this subscriber has been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        internal void AddDisposeAction(Action disposeAction)
        {
            if (disposeAction is null)
            {
                throw new ArgumentNullException(nameof(disposeAction));
            }

            lock (_disposeLock)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                _disposeActions.Add(disposeAction);
            }
        }

        /// <summary>
        /// Disposes the subscriber and removes all registrations attached to it.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed registrations owned by the subscriber.
        /// </summary>
        /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Action[] disposeActions;

            lock (_disposeLock)
            {
                if (IsDisposed)
                {
                    return;
                }

                IsDisposed = true;
                disposeActions = _disposeActions.ToArray();
                _disposeActions.Clear();
            }

            foreach (var disposeAction in disposeActions)
            {
                disposeAction();
            }
        }
    }

    /// <summary>
    /// Notification subscriber.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    public class Subscriber<TNotification> : Subscriber
        where TNotification : Notification
    {
        internal Action<TNotification> Callback { get; }

        /// <summary>
        /// Creates a subscriber that handles matching notifications synchronously.
        /// </summary>
        /// <param name="callback">The callback to invoke for each notification.</param>
        public Subscriber(Action<TNotification> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }
    }

    /// <summary>
    /// Async notification subscriber.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    public class AsyncSubscriber<TNotification> : Subscriber
        where TNotification : Notification
    {
        internal Func<TNotification, CancellationToken, Task> Callback { get; }

        /// <summary>
        /// Creates a subscriber that handles matching notifications asynchronously.
        /// </summary>
        /// <param name="callback">The callback to invoke for each notification.</param>
        public AsyncSubscriber(Func<TNotification, CancellationToken, Task> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }
    }

    /// <summary>
    /// Request subscriber.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class Subscriber<TRequest, TResponse> : Subscriber
        where TRequest : Request<TResponse>
    {
        internal Func<TRequest, Response<TResponse>> Responder { get; }

        /// <summary>
        /// Creates a subscriber that responds to matching requests synchronously.
        /// </summary>
        /// <param name="responder">The responder to invoke for each request.</param>
        public Subscriber(Func<TRequest, Response<TResponse>> responder)
        {
            Responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }
    }

    /// <summary>
    /// Async request subscriber.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class AsyncSubscriber<TRequest, TResponse> : Subscriber
        where TRequest : Request<TResponse>
    {
        internal Func<TRequest, CancellationToken, Task<Response<TResponse>>> Responder { get; }

        /// <summary>
        /// Creates a subscriber that responds to matching requests asynchronously.
        /// </summary>
        /// <param name="responder">The responder to invoke for each request.</param>
        public AsyncSubscriber(Func<TRequest, CancellationToken, Task<Response<TResponse>>> responder)
        {
            Responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }
    }
}
