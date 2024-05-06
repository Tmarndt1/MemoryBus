using EventBus.Test;

namespace MemBus.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Notify_Publish_Test()
        {
            // Arrange
            var eb = new MemoryBus();

            var hit = false;

            eb.Subscribe(new Subscriber<NotifyTest>(ev =>
            {
                hit = true;
            }));

            eb.Notify(new NotifyTest());

            Assert.True(hit);
        }

        [Fact]
        public void Notify_Unsubscribe_Test()
        {
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            var subscriber = new Subscriber<NotifyTest>(ev =>
            {
                hit = true;
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Notify(new NotifyTest());

            Assert.False(hit);
        }

        [Fact]
        public void Request_Publish()
        {
            // Arrange
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            eb.Subscribe(new Subscriber<RequestTest, bool>(ev =>
            {
                return true;
            }));

            eb.Request<RequestTest, bool>(new RequestTest((bool success) =>
            {
                hit = success;
            }));

            Assert.True(hit);
        }

        [Fact]
        public void Request_Unsubscribe()
        {
            // Arrange
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            var subscriber = new Subscriber<RequestTest, bool>(ev =>
            {
                return true;
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Request<RequestTest, bool>(new RequestTest((bool success) =>
            {
                hit = true;
            }));

            Assert.False(hit);
        }

        [Fact]
        public void Base_Request_Publish()
        {
            // Arrange
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            eb.Subscribe(new Subscriber<Request<bool>, bool>(ev =>
            {
                return true;
            }));

            eb.Request<RequestTest, bool>(new RequestTest((bool success) =>
            {
                hit = true;
            }));

            Assert.True(hit);
        }

        [Fact]
        public void Base_Request_Unsubscribe()
        {
            // Arrange
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            var subscriber = new Subscriber<Request<bool>, bool>(ev =>
            {
                return true;
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Request<RequestTest, bool>(new RequestTest((bool success) =>
            {
                hit = true;
            }));

            Assert.False(hit);
        }
    }
}