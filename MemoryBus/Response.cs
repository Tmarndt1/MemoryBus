namespace MemBus
{
    public class Response<TValue>
    {
        /// <summary>
        /// The responder to the request.
        /// </summary>
        public readonly object Responder;

        /// <summary>
        /// The value of the response.
        /// </summary>
        public readonly TValue Value;

        public Response(object responder, TValue value)
        {
            Responder = responder;
            Value = value;
        }
    }
}
