namespace MemBus
{
    public interface IMemoryBus
    {
        public void Notify<TNofication>(TNofication notification)
            where TNofication : Notification;

        public void Request<TRequest, TResponse>(TRequest request)
            where TRequest : Request<TResponse>;

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
