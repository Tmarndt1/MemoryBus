
namespace MemBus
{
    public abstract class Subscriber
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
    }

    /// <summary>
    /// Notification subscriber.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    public class Subscriber<TNotification> : Subscriber, IDisposable
        where TNotification : Notification
    {
        internal Action<TNotification> Delegate;

        private bool disposedValue;

        public Subscriber(Action<TNotification> callback)
        {
            Delegate = callback;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Request subscriber. 
    /// </summary>
    /// <typeparam name="TRequest">The type of the Request.</typeparam>
    /// <typeparam name="TResponse">The type of the Response.</typeparam>
    public class Subscriber<TRequest, TResponse> : Subscriber, IDisposable
        where TRequest : Request<TResponse>
    {
        internal Func<TRequest, Response<TResponse>> Delegate;

        private bool disposedValue;

        public Subscriber(Func<TRequest, Response<TResponse>> responder)
        {
            Delegate = responder;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
