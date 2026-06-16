# MemoryBus

MemoryBus is a small in-memory bus for .NET applications. It supports publish/subscribe notifications and request/response messages without requiring a container or external broker.

## Install

Reference the `MemoryBus` project directly, or package it with:

```powershell
dotnet pack .\MemoryBus\MemoryBus.csproj
```

## Notifications

```csharp
var bus = new MemoryBus();

using var subscription = bus.Subscribe<Notification>(notification =>
{
    Console.WriteLine(notification.Name);
});

bus.Publish(new Notification(this, "saved"));
```

Subscribers registered for a base notification type also receive derived notification types.

## Requests

Requests can receive responses from one or more responders.

```csharp
var bus = new MemoryBus();

bus.Subscribe<Request<bool>, bool>(request =>
{
    return new Response<bool>(this, true);
});

bus.Publish(new Request<bool>(this, "can-save", response =>
{
    Console.WriteLine(response.Value);
}));
```

## Async Subscribers

Async notification and request subscribers are first-class and are awaited by `PublishAsync`.

```csharp
bus.SubscribeAsync<Notification>(async (notification, token) =>
{
    await SaveAuditLogAsync(notification, token);
});

await bus.PublishAsync(new Notification(this, "saved"), cancellationToken);
```

## Unsubscribing

`Subscribe` returns an `IDisposable` subscription token. Disposing either the token or the subscriber removes the registration.

```csharp
var subscription = bus.Subscribe<Notification>(_ => { });
subscription.Dispose();
```

## Behavior

- Publishing with no subscribers is a no-op.
- Subscribers are invoked in registration order within each message type.
- Derived message subscribers are invoked before base message subscribers.
- If a subscriber throws, publishing stops and the exception is propagated to the caller.
- Subscribe and unsubscribe operations are safe while a publish is in progress.
