using Grpc.Core;
using System.Collections.Concurrent;

namespace ConsoleApp2
{

    public class ChatServiceImpl : ChatService.ChatServiceBase
    {
        public override Task<Message> SendMessage(Message request, ServerCallContext context)
        {
            Console.WriteLine($"Received message: {request.Text}");
            return Task.FromResult(new Message { Text = "Server received your message." });
        }
    }

    public class PubSubSericeImpl : PubSubService.PubSubServiceBase
    {
        private static readonly ConcurrentDictionary<string, IServerStreamWriter<Notification>> Subscribers =
    new ConcurrentDictionary<string, IServerStreamWriter<Notification>>();

        public override async Task Subscribe(SubscriptionRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            Subscribers.TryAdd(request.SubscriberId, responseStream);

            await responseStream.WriteAsync(new Notification
            {
                PublisherId = "Server",
                Message = "You are now subscribed."
            });

            while (!context.CancellationToken.IsCancellationRequested)
            {
                // Wait for cancellation or other termination conditions.
            }

            Subscribers.TryRemove(request.SubscriberId, out _);
        }

        public override async Task<Empty> Publish(PublishRequest request, ServerCallContext context)
        {
            var notification = new Notification
            {
                PublisherId = context.Peer,
                Message = request.Message
            };

            var tasks = new List<Task>();
            foreach (var subscriber in Subscribers)
            {
                tasks.Add(subscriber.Value.WriteAsync(notification));
            }

            await Task.WhenAll(tasks);

            return new Empty();
        }
    }

    public class AutionServiceImpl: AuctionService.AuctionServiceBase
    {
        public override Task<InitiateAuctionResponse> InitiateAuction(InitiateAuctionRequest request, ServerCallContext context)
        {
            // Implement the InitiateAuction logic here
            int auctionId = 1 /* Generate a unique auction ID */;

            // Return the auction ID in the response
            return Task.FromResult(new InitiateAuctionResponse { AuctionId = auctionId });
        }

        public override Task<PlaceBidResponse> PlaceBid(PlaceBidRequest request, ServerCallContext context)
        {
            // Implement the PlaceBid logic here
            bool bidAccepted = true /* Check if the bid is accepted */;

            // Return whether the bid was accepted in the response
            return Task.FromResult(new PlaceBidResponse { Accepted = bidAccepted });
        }
    }
}