
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
        internal Action<TNotification> Handler;

        private bool disposedValue;

        public Subscriber(Action<TNotification> callback)
        {
            Handler = callback;
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
        internal Func<TRequest, TResponse> Handler;

        private bool disposedValue;

        public Subscriber(Func<TRequest, TResponse> callback)
        {
            Handler = callback;
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
