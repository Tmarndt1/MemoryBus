namespace MemBus
{
    public abstract class Notify : IEvent
    {
        public abstract string Name { get; }

        public abstract object Sender { get; }
    }

    public abstract class Request<TResponse> : IEvent
    {
        public abstract string Name { get; }

        public abstract object Sender { get; }

        private readonly Action<TResponse?> _callback;

        /// <summary>
        /// Request constructor that requires a callback.
        /// </summary>
        /// <param name="callback"></param>
        protected Request(Action<TResponse?> callback)
        {
            _callback = callback;
        }

        public void Respond(TResponse? response)
        {
            _callback(response);
        }
    }
}
