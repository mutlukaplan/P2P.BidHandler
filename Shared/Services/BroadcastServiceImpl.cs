using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public class BroadcastServiceImpl : BroadcastService.BroadcastServiceBase
    {
        //    private static readonly List<IServerStreamWriter<BroadcastMessage>> Subscribers =
        //new List<IServerStreamWriter<BroadcastMessage>>();

        private static readonly List<IAsyncStreamWriter<BroadcastMessage>> Subscribers =
     new List<IAsyncStreamWriter<BroadcastMessage>>();

        public override Task<BroadcastEmpty> Broadcast(BroadcastMessage request, ServerCallContext context)
        {
            var message = request.Text;
            var auction_id = request.AuctionId;
            var ownerId= request.OwnerId;

            var response = new BroadcastMessage
            {
                Text = message,
                AuctionId = auction_id,
                OwnerId=ownerId,
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
                    if(request!=null)
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

        //public override Task<BroadcastEmpty> Unsubscribe(BroadcastEmpty request, ServerCallContext context)
        //{
        //    // Remove the client's stream writer from the list of subscribers when they unsubscribe.
        //    lock (Subscribers)
        //    {
        //        var subscriberToRemove = Subscribers.Find(s => s == context.ResponseStream);
        //        if (subscriberToRemove != null)
        //        {
        //            Subscribers.Remove(subscriberToRemove);
        //        }
        //    }

        //    return Task.FromResult(new BroadcastEmpty());
        //}

    }
}
