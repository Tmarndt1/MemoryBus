namespace MemBus
{
    public abstract class Notification
    {
        public abstract string Name { get; }

        public abstract object Sender { get; }
    }
}
