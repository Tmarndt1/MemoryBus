namespace MemBus
{
    /// <summary>
    /// Represents a request to publish on the memory bus.
    /// </summary>
    /// <typeparam name="TResponse">The type of value returned by responders.</typeparam>
    public class Request<TResponse>
    {
        /// <summary>
        /// The name of the request.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The sender of the request.
        /// </summary>
        public object Sender { get; }

        private readonly Action<Response<TResponse>> _callback;

        /// <summary>
        /// Creates a request with the supplied sender, name, and response callback.
        /// </summary>
        /// <param name="sender">The sender of the request.</param>
        /// <param name="name">The name of the request.</param>
        /// <param name="callback">The callback that receives each response.</param>
        public Request(object sender, string name, Action<Response<TResponse>> callback)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        internal void Respond(Response<TResponse> response)
        {
            _callback(response);
        }
    }
}
