namespace MemBus
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IMemoryBus"/>.
    /// </summary>
    public class MemoryBus : IMemoryBus
    {
        private readonly object _sync = new();
        private long _sequence;
        private readonly Dictionary<Type, List<NotificationRegistration>> _notificationSubscribers = new();
        private readonly Dictionary<Type, List<RequestRegistration>> _requestSubscribers = new();

        /// <inheritdoc />
        public void Publish<TNotification>(TNotification notification) where TNotification : Notification
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));
            var handlers = GetNotificationHandlers(notification.GetType());
            if (handlers.Any(static handler => handler.IsAsync))
                throw new InvalidOperationException("Synchronous publishing cannot invoke an asynchronous subscriber. Use PublishAsync instead.");

            foreach (var handler in handlers)
                handler.Callback(notification, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void Publish<TResponse>(Request<TResponse> request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            var handlers = GetRequestHandlers(request.GetType());
            if (handlers.Any(static handler => handler.IsAsync))
                throw new InvalidOperationException("Synchronous publishing cannot invoke an asynchronous responder. Use PublishAsync instead.");

            foreach (var handler in handlers)
                Respond(request, handler.Callback(request, CancellationToken.None).GetAwaiter().GetResult());
        }

        /// <inheritdoc />
        public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken token = default)
            where TNotification : Notification
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));
            foreach (var handler in GetNotificationHandlers(notification.GetType()))
            {
                token.ThrowIfCancellationRequested();
                await handler.Callback(notification, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task PublishAsync<TResponse>(Request<TResponse> request, CancellationToken token = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            foreach (var handler in GetRequestHandlers(request.GetType()))
            {
                token.ThrowIfCancellationRequested();
                Respond(request, await handler.Callback(request, token).ConfigureAwait(false));
            }
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TNotification>(Action<TNotification> callback) where TNotification : Notification =>
            Subscribe(new Subscriber<TNotification>(callback));

        /// <inheritdoc />
        public IDisposable SubscribeAsync<TNotification>(Func<TNotification, CancellationToken, Task> callback) where TNotification : Notification =>
            Subscribe(new AsyncSubscriber<TNotification>(callback));

        /// <inheritdoc />
        public IDisposable Subscribe<TNotification>(Subscriber<TNotification> subscriber) where TNotification : Notification
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            return AddNotificationSubscription(typeof(TNotification), subscriber, (notification, _) =>
            {
                subscriber.Callback((TNotification)notification);
                return Task.CompletedTask;
            }, false);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TNotification>(AsyncSubscriber<TNotification> subscriber) where TNotification : Notification
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            return AddNotificationSubscription(typeof(TNotification), subscriber,
                (notification, token) => subscriber.Callback((TNotification)notification, token), true);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TRequest, TResponse>(Func<TRequest, Response<TResponse>> responder)
            where TRequest : Request<TResponse> => Subscribe(new Subscriber<TRequest, TResponse>(responder));

        /// <inheritdoc />
        public IDisposable SubscribeAsync<TRequest, TResponse>(Func<TRequest, CancellationToken, Task<Response<TResponse>>> responder)
            where TRequest : Request<TResponse> => Subscribe(new AsyncSubscriber<TRequest, TResponse>(responder));

        /// <inheritdoc />
        public IDisposable Subscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            return AddRequestSubscription(typeof(TRequest), subscriber,
                (request, _) => Task.FromResult<object>(subscriber.Responder((TRequest)request)), false);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TRequest, TResponse>(AsyncSubscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            return AddRequestSubscription(typeof(TRequest), subscriber,
                async (request, token) => await subscriber.Responder((TRequest)request, token).ConfigureAwait(false), true);
        }

        /// <inheritdoc />
        public void Unsubscribe<TNotification>(Subscriber<TNotification> subscriber) where TNotification : Notification
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            Unsubscribe<TNotification>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TNotification>(AsyncSubscriber<TNotification> subscriber) where TNotification : Notification
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            Unsubscribe<TNotification>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            Unsubscribe<TRequest, TResponse>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TRequest, TResponse>(AsyncSubscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            Unsubscribe<TRequest, TResponse>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TNotification>(Guid id) where TNotification : Notification
        {
            lock (_sync) RemoveSubscriber(_notificationSubscribers, typeof(TNotification), id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TRequest, TResponse>(Guid id) where TRequest : Request<TResponse>
        {
            lock (_sync) RemoveSubscriber(_requestSubscribers, typeof(TRequest), id);
        }

        private IDisposable AddNotificationSubscription(Type type, Subscriber subscriber,
            Func<Notification, CancellationToken, Task> callback, bool isAsync)
        {
            var registration = new NotificationRegistration(subscriber.Id, NextSequence(), callback, isAsync);
            var subscription = new Subscription(() =>
            {
                registration.Deactivate();
                RemoveRegistration(_notificationSubscribers, type, registration);
            });
            subscriber.AddDisposeAction(subscription.Dispose);
            lock (_sync)
            {
                if (!registration.TryActivate()) throw new ObjectDisposedException(subscriber.GetType().FullName);
                AddRegistration(_notificationSubscribers, type, registration);
            }
            return subscription;
        }

        private IDisposable AddRequestSubscription(Type type, Subscriber subscriber,
            Func<object, CancellationToken, Task<object>> callback, bool isAsync)
        {
            var registration = new RequestRegistration(subscriber.Id, NextSequence(), callback, isAsync);
            var subscription = new Subscription(() =>
            {
                registration.Deactivate();
                RemoveRegistration(_requestSubscribers, type, registration);
            });
            subscriber.AddDisposeAction(subscription.Dispose);
            lock (_sync)
            {
                if (!registration.TryActivate()) throw new ObjectDisposedException(subscriber.GetType().FullName);
                AddRegistration(_requestSubscribers, type, registration);
            }
            return subscription;
        }

        private long NextSequence()
        {
            lock (_sync) return ++_sequence;
        }

        private NotificationRegistration[] GetNotificationHandlers(Type type)
        {
            lock (_sync) return GetHandlers(_notificationSubscribers, type);
        }

        private RequestRegistration[] GetRequestHandlers(Type type)
        {
            lock (_sync) return GetHandlers(_requestSubscribers, type);
        }

        private static TRegistration[] GetHandlers<TRegistration>(Dictionary<Type, List<TRegistration>> subscriptions, Type type)
            where TRegistration : Registration
        {
            var handlers = new List<TRegistration>();
            Type? currentType = type;
            do
            {
                if (subscriptions.TryGetValue(currentType, out var subscribers)) handlers.AddRange(subscribers);
                currentType = currentType.BaseType;
            } while (currentType != null);
            return handlers.ToArray();
        }

        private static void AddRegistration<TRegistration>(Dictionary<Type, List<TRegistration>> subscriptions,
            Type type, TRegistration registration)
        {
            if (!subscriptions.TryGetValue(type, out var subscribers))
            {
                subscribers = new List<TRegistration>();
                subscriptions.Add(type, subscribers);
            }
            subscribers.Add(registration);
        }

        private static void RemoveSubscriber<TRegistration>(Dictionary<Type, List<TRegistration>> subscriptions,
            Type type, Guid subscriberId) where TRegistration : Registration
        {
            if (!subscriptions.TryGetValue(type, out var subscribers)) return;
            subscribers.RemoveAll(registration => registration.SubscriberId == subscriberId);
            if (subscribers.Count == 0) subscriptions.Remove(type);
        }

        private void RemoveRegistration<TRegistration>(Dictionary<Type, List<TRegistration>> subscriptions,
            Type type, TRegistration registration) where TRegistration : Registration
        {
            lock (_sync)
            {
                if (!subscriptions.TryGetValue(type, out var subscribers)) return;
                subscribers.Remove(registration);
                if (subscribers.Count == 0) subscriptions.Remove(type);
            }
        }

        private static void Respond<TResponse>(Request<TResponse> request, object response)
        {
            if (response is not Response<TResponse> typedResponse)
                throw new InvalidOperationException("Request subscriber returned an invalid response.");
            request.Respond(typedResponse);
        }

        private abstract class Registration
        {
            private int _state;
            protected Registration(Guid subscriberId, long sequence, bool isAsync)
            {
                SubscriberId = subscriberId;
                Sequence = sequence;
                IsAsync = isAsync;
            }
            public Guid SubscriberId { get; }
            public long Sequence { get; }
            public bool IsAsync { get; }
            public bool TryActivate() => Interlocked.CompareExchange(ref _state, 1, 0) == 0;
            public void Deactivate() => Interlocked.Exchange(ref _state, 2);
        }

        private sealed class NotificationRegistration : Registration
        {
            public NotificationRegistration(Guid subscriberId, long sequence,
                Func<Notification, CancellationToken, Task> callback, bool isAsync)
                : base(subscriberId, sequence, isAsync) => Callback = callback;
            public Func<Notification, CancellationToken, Task> Callback { get; }
        }

        private sealed class RequestRegistration : Registration
        {
            public RequestRegistration(Guid subscriberId, long sequence,
                Func<object, CancellationToken, Task<object>> callback, bool isAsync)
                : base(subscriberId, sequence, isAsync) => Callback = callback;
            public Func<object, CancellationToken, Task<object>> Callback { get; }
        }

        private sealed class Subscription : IDisposable
        {
            private Action? _dispose;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}
