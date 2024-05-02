using MemBus;

namespace EventBus.Test
{
    public class NotifyTest : Notify
    {
        public override string Name => throw new NotImplementedException();

        public override object Sender => throw new NotImplementedException();
    }
}
