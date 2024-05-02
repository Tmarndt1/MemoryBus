namespace MemBus
{
    public interface IMemoryBus
    {
        public void Publish<TNofication>(TNofication notification)
            where TNofication : Notify;

        public void Publish<TRequest, TResponse>(TRequest request)
            where TRequest : Request<TResponse>;

        public void Subscribe<TNofication>(Observer<TNofication> subscriber)
            where TNofication : Notify;

        public void Subscribe<TRequest, TResponse>(Observer<TRequest, TResponse> subscriber)
            where TRequest: Request<TResponse>;

        public void Unsubscribe<TNotification>(Observer<TNotification> subscriber)
            where TNotification : Notify;

        public void Unsubscribe<TRequest, TResponse>(Observer<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>;

        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notify;

        public void Unsubscribe<TRequest, TResponse>(Guid id)
            where TRequest : Request<TResponse>;
    }
}
