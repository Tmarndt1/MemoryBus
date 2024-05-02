
namespace MemBus
{
    public interface IEvent
    {
        /// <summary>
        /// The event name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The sender of the event.
        /// </summary>
        public object Sender { get; }
    }
}
