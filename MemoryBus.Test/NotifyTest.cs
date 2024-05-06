using MemBus;

namespace EventBus.Test
{
    public class NotifyTest : Notification
    {
        public override string Name => throw new NotImplementedException();

        public override object Sender => throw new NotImplementedException();
    }
}
