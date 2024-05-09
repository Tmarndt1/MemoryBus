using System.Diagnostics;

namespace MemBus
{
    public class MemoryBus : IMemoryBus
    {
        private readonly Dictionary<Type, Dictionary<Guid, Delegate>> _notifySubs = new();

        private readonly Dictionary<Type, Dictionary<Guid, Delegate>> _requestSubs = new();

        public void Publish<TNofication>(TNofication notification) 
            where TNofication : Notification
        {
            Type? type = typeof(TNofication);

            do
            {
                if (_notifySubs.TryGetValue(type, out var dictionary))
                {
                    foreach (var @delegate in dictionary.Values)
                    {
                        @delegate.Method.Invoke(@delegate.Target, new object[] { notification });
                    }
                }

                type = type.BaseType;

            } while (type != null);
        }

        public void Publish<TResponse>(Request<TResponse> request) 
        {
            Type? type = request.GetType();

            do
            {
                if (_requestSubs.TryGetValue(type, out var dictionary))
                {
                    foreach (var @delegate in dictionary.Values)
                    {
                        Response<TResponse>? response = (Response<TResponse>?)@delegate.Method.Invoke(@delegate.Target, new object[] { request });

                        request.Respond(response);
                    }
                }

                type = type.BaseType;
            } while (type != null);
        }

        public Task PublishAsync<TNofication>(TNofication notification, CancellationToken token = default) where TNofication : Notification
        {
            return Task.Run(() => Publish(notification), token);
        }

        public Task PublishAsync<TResponse>(Request<TResponse> request, CancellationToken token = default)
        {
            return Task.Run(() => Publish(request), token);
        }

        public void Subscribe<TNotification>(Subscriber<TNotification> subscriber) 
            where TNotification : Notification
        {
            Type type = typeof(TNotification);

            if (_notifySubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Add(subscriber.Id, subscriber.Delegate);
            }
            else
            {
                _notifySubs.Add(type, new Dictionary<Guid, Delegate>()
                {
                    { subscriber.Id, subscriber.Delegate }
                });
            }
        }

        public void Subscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>
        {
            Type type = typeof(TRequest);

            if (_requestSubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Add(subscriber.Id, subscriber.Delegate);
            }
            else
            {
                _requestSubs.Add(type, new Dictionary<Guid, Delegate>()
                {
                    { subscriber.Id, subscriber.Delegate }
                });
            }
        }

        public void Unsubscribe<TNotification>(Subscriber<TNotification> subscriber) 
            where TNotification : Notification
        {
            Type type = typeof(TNotification);

            if (_notifySubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Remove(subscriber.Id);
            }
        }

        public void Unsubscribe<TRequest, TResponse>(Subscriber<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>
        {
            Type type = typeof(TRequest);

            if (_requestSubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Remove(subscriber.Id);
            }
        }

        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notification
        {
            if (_notifySubs.TryGetValue(typeof(TNotification), out var dictionary))
            {
                dictionary.Remove(id);
            }
        }

        public void Unsubscribe<TRequest, TResponse>(Guid id)
            where TRequest : Request<TResponse>
        {
            if (_requestSubs.TryGetValue(typeof(TRequest), out var dictionary))
            {
                dictionary.Remove(id);
            }
        }
    }
}