namespace MemBus.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Publish_Notifies_Subscriber()
        {
            var bus = new MemoryBus();
            var hit = false;

            bus.Subscribe<Notification>(notification =>
            {
                hit = true;
            });

            bus.Publish(new Notification(this, "notification"));

            Assert.True(hit);
        }

        [Fact]
        public void Subscription_Dispose_Unsubscribes()
        {
            var bus = new MemoryBus();
            var hit = false;

            var subscription = bus.Subscribe<Notification>(notification =>
            {
                hit = true;
            });

            subscription.Dispose();

            bus.Publish(new Notification(this, "notification"));

            Assert.False(hit);
        }

        [Fact]
        public void Subscriber_Dispose_Unsubscribes()
        {
            var bus = new MemoryBus();
            var hit = false;
            var subscriber = new Subscriber<Notification>(notification =>
            {
                hit = true;
            });

            bus.Subscribe(subscriber);

            subscriber.Dispose();

            bus.Publish(new Notification(this, "notification"));

            Assert.False(hit);
        }

        [Fact]
        public void Unsubscribe_By_Id_Removes_Subscriber()
        {
            var bus = new MemoryBus();
            var hit = false;
            var subscriber = new Subscriber<Notification>(notification =>
            {
                hit = true;
            });

            bus.Subscribe(subscriber);
            bus.Unsubscribe<Notification>(subscriber.Id);

            bus.Publish(new Notification(this, "notification"));

            Assert.False(hit);
        }

        [Fact]
        public void Publish_Allows_Unsubscribe_During_Callback()
        {
            var bus = new MemoryBus();
            var hitCount = 0;
            Subscriber<Notification>? subscriber = null;

            subscriber = new Subscriber<Notification>(notification =>
            {
                hitCount++;
                bus.Unsubscribe(subscriber!);
            });

            bus.Subscribe(subscriber);

            bus.Publish(new Notification(this, "notification"));
            bus.Publish(new Notification(this, "notification"));

            Assert.Equal(1, hitCount);
        }

        [Fact]
        public void Publish_Derived_Notification_Reaches_Base_Subscriber()
        {
            var bus = new MemoryBus();
            var hit = false;

            bus.Subscribe<Notification>(notification =>
            {
                hit = true;
            });

            bus.Publish(new DerivedNotification(this, "notification"));

            Assert.True(hit);
        }

        [Fact]
        public void Publish_With_No_Subscribers_Does_Not_Throw()
        {
            var bus = new MemoryBus();

            bus.Publish(new Notification(this, "notification"));
        }

        [Fact]
        public void Publish_Request_Calls_Response_Callback()
        {
            var bus = new MemoryBus();
            var hit = false;

            bus.Subscribe<Request<bool>, bool>(request =>
            {
                return new Response<bool>(this, true);
            });

            bus.Publish(new Request<bool>(this, "request", response =>
            {
                hit = response.Value;
            }));

            Assert.True(hit);
        }

        [Fact]
        public void Publish_Request_Can_Receive_Multiple_Responses()
        {
            var bus = new MemoryBus();
            var responses = new List<int>();

            bus.Subscribe<Request<int>, int>(request => new Response<int>(this, 1));
            bus.Subscribe<Request<int>, int>(request => new Response<int>(this, 2));

            bus.Publish(new Request<int>(this, "request", response =>
            {
                responses.Add(response.Value);
            }));

            Assert.Equal(new[] { 1, 2 }, responses);
        }

        [Fact]
        public void Request_Unsubscribe_Removes_Responder()
        {
            var bus = new MemoryBus();
            var hit = false;
            var subscriber = new Subscriber<Request<bool>, bool>(request =>
            {
                return new Response<bool>(this, true);
            });

            bus.Subscribe(subscriber);

            bus.Unsubscribe(subscriber);

            bus.Publish(new Request<bool>(this, "request", response =>
            {
                hit = true;
            }));

            Assert.False(hit);
        }

        [Fact]
        public async Task PublishAsync_Awaits_Async_Notification_Subscriber()
        {
            var bus = new MemoryBus();
            var hit = false;

            bus.SubscribeAsync<Notification>(async (notification, token) =>
            {
                await Task.Delay(1, token);
                hit = true;
            });

            await bus.PublishAsync(new Notification(this, "notification"));

            Assert.True(hit);
        }

        [Fact]
        public async Task PublishAsync_Awaits_Async_Request_Subscriber()
        {
            var bus = new MemoryBus();
            var hit = false;

            bus.SubscribeAsync<Request<bool>, bool>(async (request, token) =>
            {
                await Task.Delay(1, token);
                return new Response<bool>(this, true);
            });

            await bus.PublishAsync(new Request<bool>(this, "request", response =>
            {
                hit = response.Value;
            }));

            Assert.True(hit);
        }

        [Fact]
        public async Task PublishAsync_Observes_Cancellation()
        {
            var bus = new MemoryBus();
            using var source = new CancellationTokenSource();

            bus.SubscribeAsync<Notification>((notification, token) => Task.CompletedTask);
            source.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                bus.PublishAsync(new Notification(this, "notification"), source.Token));
        }

        [Fact]
        public void Null_Arguments_Throw()
        {
            var bus = new MemoryBus();
            Request<bool>? request = null;

            Assert.Throws<ArgumentNullException>(() => bus.Publish(request!));
            Assert.Throws<ArgumentNullException>(() => bus.Subscribe<Notification>((Action<Notification>)null!));
            Assert.Throws<ArgumentNullException>(() => new Notification(null!, "notification"));
            Assert.Throws<ArgumentNullException>(() => new Request<bool>(this, "request", null!));
            Assert.Throws<ArgumentNullException>(() => new Response<bool>(null!, true));
        }

        private sealed class DerivedNotification : Notification
        {
            public DerivedNotification(object sender, string name)
                : base(sender, name)
            {
            }
        }
    }
}
