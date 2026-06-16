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
        public string Name { get; }

        /// <summary>
        /// The sender of the notification.
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// Creates a notification with the supplied sender and name.
        /// </summary>
        /// <param name="sender">The sender of the notification.</param>
        /// <param name="name">The name of the notification.</param>
        public Notification(object sender, string name)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
