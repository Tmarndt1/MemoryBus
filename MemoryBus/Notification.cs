namespace MemBus
{
    /// <summary>
    /// Represents a notification to publish on the memory bus.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// The name of the notification.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The sender of the notification.
        /// </summary>
        public readonly object Sender;

        /// <summary>
        /// Constructor that sets the sender and name of the notification.
        /// </summary>
        /// <param name="sender">The sender of the notification.</param>
        /// <param name="name">The name of the notification.</param>
        public Notification(object sender, string name)
        {
            Sender = sender;
            Name = name;
        }
    }
}
