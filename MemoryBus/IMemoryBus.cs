namespace MemBus
{
    /// <summary>
    /// Publishes in-process notifications and request/response messages to registered subscribers.
    /// </summary>
    public interface IMemoryBus
    {
        /// <summary>
        /// Publishes a notification to subscribers registered for its concrete type or any base notification type.
        /// </summary>
        /// <typeparam name="TNotification">The notification type.</typeparam>
        /// <param name="notification">The notification to publish.</param>
        public void Publish<TNotification>(TNotification notification)
            where TNotification : Notification;

        /// <summary>
        /// Publishes a request to responders registered for its concrete type or any base request type.
        /// </summary>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="request">The request to publish.</param>
        public void Publish<TResponse>(Request<TResponse> request);

        /// <summary>
        /// Publishes a notification and awaits each async subscriber.
        /// </summary>
        /// <typeparam name="TNotification">The notification type.</typeparam>
        /// <param name="notification">The notification to publish.</param>
        /// <param name="token">A token used to cancel publication before invoking the next subscriber.</param>
        public Task PublishAsync<TNotification>(TNotification notification, CancellationToken token = default)
            where TNotification : Notification;

        /// <summary>
        /// Publishes a request and awaits each async responder.
        /// </summary>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="request">The request to publish.</param>
        /// <param name="token">A token used to cancel publication before invoking the next responder.</param>
        public Task PublishAsync<TResponse>(Request<TResponse> request, CancellationToken token = default);

        /// <summary>
        /// Registers a synchronous notification callback.
        /// </summary>
        /// <typeparam name="TNotification">The notification type to receive.</typeparam>
        /// <param name="callback">The callback to invoke for matching notifications.</param>
        /// <returns>A subscription token that unregisters the callback when disposed.</returns>
        public IDisposable Subscribe<TNotification>(Action<TNotification> callback)
            where TNotification : Notification;

        /// <summary>
        /// Registers an asynchronous notification callback.
        /// </summary>
        /// <typeparam name="TNotification">The notification type to receive.</typeparam>
        /// <param name="callback">The callback to invoke for matching notifications.</param>
        /// <returns>A subscription token that unregisters the callback when disposed.</returns>
        public IDisposable SubscribeAsync<TNotification>(Func<TNotification, CancellationToken, Task> callback)
            where TNotification : Notification;

        /// <summary>
        /// Registers a notification subscriber instance.
        /// </summary>
        /// <typeparam name="TNotification">The notification type to receive.</typeparam>
        /// <param name="subscriber">The subscriber to register.</param>
        /// <returns>A subscription token that unregisters the subscriber when disposed.</returns>
        public IDisposable Subscribe<TNotification>(Subscriber<TNotification> subscriber)
            where TNotification : Notification;

        /// <summary>
        /// Registers a synchronous request responder.
        /// </summary>
        /// <typeparam name="TRequest">The request type to receive.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="responder">The responder to invoke for matching requests.</param>
        /// <returns>A subscription token that unregisters the responder when disposed.</returns>
        public IDisposable Subscribe<TRequest, TResponse>(Func<TRequest, Response<TResponse>> responder)
            where TRequest : Request<TResponse>;

        /// <summary>
        /// Registers an asynchronous request responder.
        /// </summary>
        /// <typeparam name="TRequest">The request type to receive.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="responder">The responder to invoke for matching requests.</param>
        /// <returns>A subscription token that unregisters the responder when disposed.</returns>
        public IDisposable SubscribeAsync<TRequest, TResponse>(Func<TRequest, CancellationToken, Task<Response<TResponse>>> responder)
            where TRequest : Request<TResponse>;

        /// <summary>
        /// Registers a request subscriber instance.
        /// </summary>
        /// <typeparam name="TRequest">The request type to receive.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="subscriber">The subscriber to register.</param>
        /// <returns>A subscription token that unregisters the subscriber when disposed.</returns>
        public IDisposable Subscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>;

        /// <summary>
        /// Removes a notification subscriber.
        /// </summary>
        /// <typeparam name="TNotification">The notification type used when the subscriber was registered.</typeparam>
        /// <param name="subscriber">The subscriber to remove.</param>
        public void Unsubscribe<TNotification>(Subscriber<TNotification> subscriber)
            where TNotification : Notification;

        /// <summary>
        /// Removes a request subscriber.
        /// </summary>
        /// <typeparam name="TRequest">The request type used when the subscriber was registered.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="subscriber">The subscriber to remove.</param>
        public void Unsubscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest : Request<TResponse>;

        /// <summary>
        /// Removes a notification subscriber by identifier.
        /// </summary>
        /// <typeparam name="TNotification">The notification type used when the subscriber was registered.</typeparam>
        /// <param name="id">The subscriber identifier.</param>
        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notification;

        /// <summary>
        /// Removes a request subscriber by identifier.
        /// </summary>
        /// <typeparam name="TRequest">The request type used when the subscriber was registered.</typeparam>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="id">The subscriber identifier.</param>
        public void Unsubscribe<TRequest, TResponse>(Guid id)
            where TRequest : Request<TResponse>;
    }
}
