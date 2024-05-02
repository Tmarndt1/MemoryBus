using System.Reflection;

namespace MemBus
{
    public class MemoryBus : IMemoryBus
    {
        private readonly Dictionary<Type, Dictionary<Guid, Tuple<object?, MethodInfo>>> _notifySubs = new();

        private readonly Dictionary<Type, Dictionary<Guid, Tuple<object?, MethodInfo>>> _requestSubs = new();

        public void Publish<TNofication>(TNofication notification) 
            where TNofication : Notify
        {
            Type type = typeof(TNofication);

            if (_notifySubs.TryGetValue(type, out var dictionary))
            {
                foreach (var tuple in dictionary.Values)
                {
                    tuple.Item2.Invoke(tuple.Item1, new object[] { notification });
                }
            }
        }

        public void Publish<TRequest, TResponse>(TRequest request) 
            where TRequest : Request<TResponse>
        {
            Type type = typeof(TRequest);

            if (_requestSubs.TryGetValue(type, out var dictionary))
            {
                foreach (var tuple in dictionary.Values)
                {
                    TResponse? response = (TResponse?)tuple.Item2.Invoke(tuple.Item1, new object[] { request });

                    request.Respond(response);
                }
            }
        }

        public void Subscribe<TNotification>(Observer<TNotification> subscriber) 
            where TNotification : Notify
        {
            Type type = typeof(TNotification);

            if (_notifySubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Add(subscriber.Id, new Tuple<object?, MethodInfo>(subscriber.Handler.Target, subscriber.Handler.Method));
            }
            else
            {
                _notifySubs.Add(type, new Dictionary<Guid, Tuple<object?, MethodInfo>>()
                {
                    { subscriber.Id, new Tuple<object?, MethodInfo>(subscriber.Handler.Target, subscriber.Handler.Method) }
                });
            }
        }

        public void Subscribe<TRequest, TResponse>(Observer<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>
        {
            Type type = typeof(TRequest);

            if (_requestSubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Add(subscriber.Id, new Tuple<object?, MethodInfo>(subscriber.Handler.Target, subscriber.Handler.Method));
            }
            else
            {
                _requestSubs.Add(type, new Dictionary<Guid, Tuple<object?, MethodInfo>>()
                {
                    { subscriber.Id, new Tuple<object?, MethodInfo>(subscriber.Handler.Target, subscriber.Handler.Method) }
                });
            }
        }

        public void Unsubscribe<TNotification>(Observer<TNotification> subscriber) 
            where TNotification : Notify
        {
            Type type = typeof(TNotification);

            if (_notifySubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Remove(subscriber.Id);
            }
        }

        public void Unsubscribe<TRequest, TResponse>(Observer<TRequest, TResponse> subscriber) 
            where TRequest : Request<TResponse>
        {
            Type type = typeof(TRequest);

            if (_requestSubs.TryGetValue(type, out var dictionary))
            {
                dictionary.Remove(subscriber.Id);
            }
        }

        public void Unsubscribe<TNotification>(Guid id)
            where TNotification : Notify
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