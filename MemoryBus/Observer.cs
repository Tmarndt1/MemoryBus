
namespace MemBus
{
    public abstract class Observer
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
    }

    public class Observer<TNotification> : Observer, IDisposable
        where TNotification : Notify
    {
        internal Action<TNotification> Handler;

        private bool disposedValue;

        public Observer(Action<TNotification> callback)
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

    public class Observer<TRequest, TResponse> : Observer, IDisposable
        where TRequest : Request<TResponse>
    {
        internal Func<TRequest, TResponse> Handler;

        private bool disposedValue;

        public Observer(Func<TRequest, TResponse> callback)
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
