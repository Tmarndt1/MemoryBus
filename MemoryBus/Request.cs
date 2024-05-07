namespace MemBus
{
    public abstract class Request<TValue>
    {
        public abstract string Name { get; }

        public abstract object Sender { get; }


        private readonly Action<Response<TValue>> _callback;

        /// <summary>
        /// Request constructor that requires a callback.
        /// </summary>
        /// <param name="callback"></param>
        protected Request(Action<Response<TValue>> callback)
        {
            _callback = callback;
        }

        public void Respond(Response<TValue> response)
        {
            _callback(response);
        }
    }
}
