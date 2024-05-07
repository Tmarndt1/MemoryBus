namespace MemBus
{
    public class RequestTest : Request<bool>
    {
        public override string Name => throw new NotImplementedException();

        public override object Sender => throw new NotImplementedException();

        public RequestTest(Action<Response<bool>> callback) : base(callback)
        {

        }
    }
}
