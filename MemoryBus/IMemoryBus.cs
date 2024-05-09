using System.Runtime.CompilerServices;

namespace MemBus
{
    public interface IMemoryBus
    {
        public void Publish<TNofication>(TNofication notification)
            where TNofication : Notification;

        public void Publish<TResponse>(Request<TResponse> request);

        public Task PublishAsync<TNofication>(TNofication notification, CancellationToken token = default)
            where TNofication : Notification;

        public Task PublishAsync<TResponse>(Request<TResponse> request, CancellationToken token = default);

        public void Subscribe<TNofication>(Subscriber<TNofication> subscriber)
            where TNofication : Notification;

        public void Subscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber)
            where TRequest: Request<TResponse>;

        public void Unsubscribe<TNotification>(Subscriber<TNotification> subscriber)
            where TNotification : Notification;

        public void Unsubscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>;

        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notification;

        public void Unsubscribe<TRequest, TResponse>(Guid id)
            where TRequest : Request<TResponse>;
    }
}
