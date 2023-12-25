using Grpc.Core;

namespace Shared.Services
{
    public class BroadcastServiceImpl : BroadcastService.BroadcastServiceBase
    {
        private static readonly List<IAsyncStreamWriter<BroadcastMessage>> Subscribers =
                new List<IAsyncStreamWriter<BroadcastMessage>>();

        public override Task<BroadcastEmpty> Broadcast(BroadcastMessage request, ServerCallContext context)
        {
            var message = request.Text;
            var auction_id = request.AuctionId;
            var ownerId = request.OwnerId;
            var item_name = request.ItemName;
            var starting_price = request.StartingPrice;
            var bidder = request.Bidder;

            var response = new BroadcastMessage
            {
                Text = message,
                AuctionId = auction_id,
                OwnerId = ownerId,
                ItemName = item_name,
                StartingPrice = starting_price,
                Bidder = bidder,
            };

            // Broadcast the message to all connected clients.
            foreach (var subscriber in Subscribers)
            {
                subscriber.WriteAsync(response).Wait();
            }

            return Task.FromResult(new BroadcastEmpty());
        }

        public override async Task ReceiveBroadcast(BroadcastEmpty request, IServerStreamWriter<BroadcastMessage> responseStream, ServerCallContext context)
        {
            // Add the client's stream writer to the list of subscribers.
            lock (Subscribers)
            {
                Subscribers.Add(responseStream);
            }

            try
            {
                // Keep the connection open and send messages as they arrive.
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1)); // Simulate some processing time.

                    var message = new BroadcastMessage
                    {
                        Text = ""// $"Broadcasted message at {DateTime.Now}"
                    };

                    // Send the message to the client.
                    if (request != null)
                        await responseStream.WriteAsync(message);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                // The client disconnected or canceled the subscription.
            }
            finally
            {
                // Remove the client's stream writer from the list of subscribers when they unsubscribe.
                lock (Subscribers)
                {
                    Subscribers.Remove(responseStream);
                }
            }
        }
    }
}
