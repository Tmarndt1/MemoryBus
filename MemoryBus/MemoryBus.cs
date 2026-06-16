namespace MemBus
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IMemoryBus"/>.
    /// </summary>
    public class MemoryBus : IMemoryBus
    {
        private readonly object _sync = new();

        private readonly Dictionary<Type, Dictionary<Guid, Func<Notification, CancellationToken, Task>>> _notificationSubscribers = new();

        private readonly Dictionary<Type, Dictionary<Guid, Func<object, CancellationToken, Task<object>>>> _requestSubscribers = new();

        /// <inheritdoc />
        public void Publish<TNotification>(TNotification notification)
            where TNotification : Notification
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            foreach (var handler in GetNotificationHandlers(notification.GetType()))
            {
                handler(notification, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        /// <inheritdoc />
        public void Publish<TResponse>(Request<TResponse> request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            foreach (var handler in GetRequestHandlers(request.GetType()))
            {
                var result = handler(request, CancellationToken.None).GetAwaiter().GetResult();

                if (result is not Response<TResponse> response)
                {
                    throw new InvalidOperationException("Request subscriber returned an invalid response.");
                }

                request.Respond(response);
            }
        }

        /// <inheritdoc />
        public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken token = default)
            where TNotification : Notification
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            foreach (var handler in GetNotificationHandlers(notification.GetType()))
            {
                token.ThrowIfCancellationRequested();
                await handler(notification, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task PublishAsync<TResponse>(Request<TResponse> request, CancellationToken token = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            foreach (var handler in GetRequestHandlers(request.GetType()))
            {
                token.ThrowIfCancellationRequested();
                var result = await handler(request, token).ConfigureAwait(false);

                if (result is not Response<TResponse> response)
                {
                    throw new InvalidOperationException("Request subscriber returned an invalid response.");
                }

                request.Respond(response);
            }
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TNotification>(Action<TNotification> callback)
            where TNotification : Notification
        {
            return Subscribe(new Subscriber<TNotification>(callback));
        }

        /// <inheritdoc />
        public IDisposable SubscribeAsync<TNotification>(Func<TNotification, CancellationToken, Task> callback)
            where TNotification : Notification
        {
            return Subscribe(new AsyncSubscriber<TNotification>(callback));
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TNotification>(Subscriber<TNotification> subscriber)
            where TNotification : Notification
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Func<Notification, CancellationToken, Task> handler = (notification, _) =>
            {
                subscriber.Callback((TNotification)notification);
                return Task.CompletedTask;
            };

            return AddNotificationSubscription(typeof(TNotification), subscriber, handler);
        }

        /// <summary>
        /// Registers an asynchronous notification subscriber instance.
        /// </summary>
        /// <typeparam name="TNotification">The notification type to receive.</typeparam>
        /// <param name="subscriber">The subscriber to register.</param>
        /// <returns>A subscription token that unregisters the subscriber when disposed.</returns>
        public IDisposable Subscribe<TNotification>(AsyncSubscriber<TNotification> subscriber)
            where TNotification : Notification
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Func<Notification, CancellationToken, Task> handler =
                (notification, token) => subscriber.Callback((TNotification)notification, token);

            return AddNotificationSubscription(typeof(TNotification), subscriber, handler);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TRequest, TResponse>(Func<TRequest, Response<TResponse>> responder)
            where TRequest : Request<TResponse>
        {
            return Subscribe(new Subscriber<TRequest, TResponse>(responder));
        }

        /// <inheritdoc />
        public IDisposable SubscribeAsync<TRequest, TResponse>(Func<TRequest, CancellationToken, Task<Response<TResponse>>> responder)
            where TRequest : Request<TResponse>
        {
            return Subscribe(new AsyncSubscriber<TRequest, TResponse>(responder));
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Func<object, CancellationToken, Task<object>> handler = (request, _) =>
            {
                Response<TResponse> response = subscriber.Responder((TRequest)request);
                return Task.FromResult<object>(response);
            };

            return AddRequestSubscription(typeof(TRequest), subscriber, handler);
        }

        /// <summary>
        /// Registers an asynchronous request subscriber instance.
        /// </summary>
        /// <typeparam name="TRequest">The request type to receive.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="subscriber">The subscriber to register.</param>
        /// <returns>A subscription token that unregisters the subscriber when disposed.</returns>
        public IDisposable Subscribe<TRequest, TResponse>(AsyncSubscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            async Task<object> Handler(object request, CancellationToken token)
            {
                return await subscriber.Responder((TRequest)request, token).ConfigureAwait(false);
            }

            return AddRequestSubscription(typeof(TRequest), subscriber, Handler);
        }

        /// <inheritdoc />
        public void Unsubscribe<TNotification>(Subscriber<TNotification> subscriber)
            where TNotification : Notification
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Unsubscribe<TNotification>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Unsubscribe<TRequest, TResponse>(subscriber.Id);
        }

        /// <inheritdoc />
        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notification
        {
            lock (_sync)
            {
                RemoveSubscription(_notificationSubscribers, typeof(TNotification), id);
            }
        }

        /// <inheritdoc />
        public void Unsubscribe<TRequest, TResponse>(Guid id)
            where TRequest : Request<TResponse>
        {
            lock (_sync)
            {
                RemoveSubscription(_requestSubscribers, typeof(TRequest), id);
            }
        }

        private IDisposable AddNotificationSubscription(
            Type type,
            Subscriber subscriber,
            Func<Notification, CancellationToken, Task> handler)
        {
            return AddSubscription(_notificationSubscribers, type, subscriber, handler);
        }

        private IDisposable AddRequestSubscription(
            Type type,
            Subscriber subscriber,
            Func<object, CancellationToken, Task<object>> handler)
        {
            return AddSubscription(_requestSubscribers, type, subscriber, handler);
        }

        private IDisposable AddSubscription<THandler>(
            Dictionary<Type, Dictionary<Guid, THandler>> subscriptions,
            Type type,
            Subscriber subscriber,
            THandler handler)
        {
            lock (_sync)
            {
                if (!subscriptions.TryGetValue(type, out var subscribers))
                {
                    subscribers = new Dictionary<Guid, THandler>();
                    subscriptions.Add(type, subscribers);
                }

                subscribers.Add(subscriber.Id, handler);

                var subscription = new Subscription(() =>
                {
                    lock (_sync)
                    {
                        RemoveSubscription(subscriptions, type, subscriber.Id);
                    }
                });

                subscriber.AddDisposeAction(subscription.Dispose);

                return subscription;
            }
        }

        private Func<Notification, CancellationToken, Task>[] GetNotificationHandlers(Type type)
        {
            lock (_sync)
            {
                return GetHandlers(_notificationSubscribers, type);
            }
        }

        private Func<object, CancellationToken, Task<object>>[] GetRequestHandlers(Type type)
        {
            lock (_sync)
            {
                return GetHandlers(_requestSubscribers, type);
            }
        }

        private static THandler[] GetHandlers<THandler>(
            Dictionary<Type, Dictionary<Guid, THandler>> subscriptions,
            Type type)
        {
            var handlers = new List<THandler>();
            Type? currentType = type;

            do
            {
                if (subscriptions.TryGetValue(currentType, out var subscribers))
                {
                    handlers.AddRange(subscribers.Values);
                }

                currentType = currentType.BaseType;
            } while (currentType != null);

            return handlers.ToArray();
        }

        private static void RemoveSubscription<THandler>(
            Dictionary<Type, Dictionary<Guid, THandler>> subscriptions,
            Type type,
            Guid id)
        {
            if (!subscriptions.TryGetValue(type, out var subscribers))
            {
                return;
            }

            subscribers.Remove(id);

            if (subscribers.Count == 0)
            {
                subscriptions.Remove(type);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _dispose;
            private int _isDisposed;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                {
                    _dispose();
                }
            }
        }
    }
}
