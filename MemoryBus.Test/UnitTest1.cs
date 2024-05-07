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

            eb.Publish(new NotifyTest());

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

            eb.Publish(new NotifyTest());

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
                return new Response<bool>(this, true);
            }));

            eb.Publish(new RequestTest((response) =>
            {
                hit = response.Value;
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
                return new Response<bool>(this, true);
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Publish(new RequestTest((response) =>
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
                return new Response<bool>(this, true);
            }));

            eb.Publish(new RequestTest((response) =>
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
                return new Response<bool>(this, true);
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Publish(new RequestTest((response) =>
            {
                hit = response.Value;
            }));

            Assert.False(hit);
        }

        [Fact]
        public async Task Async_Test()
        {
            // Arrange
            MemoryBus eb = new MemoryBus();

            bool hit = false;

            eb.Subscribe(new Subscriber<RequestTest, bool>(ev =>
            {
                return new Response<bool>(this, true);
            }));

            await eb.PublishAsync(new RequestTest((response) =>
            {
                hit = true;
            }));

            Assert.True(hit);
        }
    }
}