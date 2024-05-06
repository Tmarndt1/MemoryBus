using MemBus;

namespace EventBus.Test
{
    public class RequestTest : Request<bool>
    {
        public override string Name => throw new NotImplementedException();

        public override object Sender => throw new NotImplementedException();

        public RequestTest(Action<bool> callback) : base(callback)
        {

        }
    }
}
