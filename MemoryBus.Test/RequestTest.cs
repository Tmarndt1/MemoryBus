using MemBus;

namespace EventBus.Test
{
    public class RequestTest<TResponse> : Request<TResponse>
    {
        public override string Name => throw new NotImplementedException();

        public override object Sender => throw new NotImplementedException();

        public RequestTest(Action<TResponse?> callback) : base(callback)
        {

        }
    }
}
