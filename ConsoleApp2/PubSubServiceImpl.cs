using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;

namespace ConsoleApp2
{
    //public class PubSubServiceImpl : PubSubService.
    //{
    //    private static readonly ConcurrentDictionary<string, IServerStreamWriter<Notification>> Subscribers =
    //        new ConcurrentDictionary<string, IServerStreamWriter<Notification>>();

    //    public override async Task Subscribe(SubscriptionRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
    //    {
    //        Subscribers.TryAdd(request.SubscriberId, responseStream);

    //        await responseStream.WriteAsync(new Notification
    //        {
    //            PublisherId = "Server",
    //            Message = "You are now subscribed."
    //        });

    //        while (!context.CancellationToken.IsCancellationRequested)
    //        {
    //            // Wait for cancellation or other termination conditions.
    //        }

    //        Subscribers.TryRemove(request.SubscriberId, out _);
    //    }

    //    public override async Task<Empty> Publish(PublishRequest request, ServerCallContext context)
    //    {
    //        var notification = new Notification
    //        {
    //            PublisherId = context.Peer,
    //            Message = request.Message
    //        };

    //        var tasks = new List<Task>();
    //        foreach (var subscriber in Subscribers)
    //        {
    //            tasks.Add(subscriber.Value.WriteAsync(notification));
    //        }

    //        await Task.WhenAll(tasks);

    //        return new Empty();
    //    }
    //}
}
