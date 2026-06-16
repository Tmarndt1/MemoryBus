namespace MemBus
{
    /// <summary>
    /// Represents a response produced by a request responder.
    /// </summary>
    /// <typeparam name="TValue">The type of value carried by the response.</typeparam>
    public class Response<TValue>
    {
        /// <summary>
        /// The responder to the request.
        /// </summary>
        public object Responder { get; }

        /// <summary>
        /// The value of the response.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Creates a response with the supplied responder and value.
        /// </summary>
        /// <param name="responder">The object that produced the response.</param>
        /// <param name="value">The response value.</param>
        public Response(object responder, TValue value)
        {
            Responder = responder ?? throw new ArgumentNullException(nameof(responder));
            Value = value;
        }
    }
}
