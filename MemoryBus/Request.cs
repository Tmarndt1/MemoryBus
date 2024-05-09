namespace MemBus
{
    /// <summary>
    /// Represents a request to publish on the memory bus.
    /// </summary>
    /// <typeparam name="TValue">The type of the value anticipated in the response(s).</typeparam>
    public class Request<TValue>
    {
        /// <summary>
        /// The name of the request.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The sender of the request.
        /// </summary>
        public readonly object Sender;

        private readonly Action<Response<TValue>> _callback;

        /// <summary>
        /// Constructor that sets the sender, name, and callback of the request.
        /// </summary>
        /// <param name="sender">The sender of the request.</param>
        /// <param name="name">The name of the request.</param>
        /// <param name="callback">The callback capture the response from reciever.</param>
        public Request(object sender, string name, Action<Response<TValue>> callback)
        {
            Sender = sender;
            Name = name;

            _callback = callback;
        }

        internal void Respond(Response<TValue> response)
        {
            _callback(response);
        }
    }
}
