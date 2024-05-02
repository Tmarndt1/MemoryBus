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

            eb.Subscribe(new Observer<NotifyTest>(ev =>
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

            var subscriber = new Observer<NotifyTest>(ev =>
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

            eb.Subscribe(new Observer<RequestTest<bool>, bool>(ev =>
            {
                return true;
            }));

            eb.Publish<RequestTest<bool>, bool>(new RequestTest<bool>((bool success) =>
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

            var subscriber = new Observer<RequestTest<bool>, bool>(ev =>
            {
                return true;
            });

            eb.Subscribe(subscriber);

            eb.Unsubscribe(subscriber);

            eb.Publish<RequestTest<bool>, bool>(new RequestTest<bool>((bool success) =>
            {
                hit = true;
            }));

            Assert.False(hit);
        }
    }
}