using Grpc.Core;
using Grpc.Net.Client;

namespace Shared.Services
{
    public class AutionServiceImpl : AuctionService.AuctionServiceBase
    {
        private readonly IAuctionCache _auctionCache;
        public  BroadcastService.BroadcastServiceClient? broadcastClient;
        const int MainChannelPort = 50055;
        public  GrpcChannel? mainChannel;

        public AutionServiceImpl(IAuctionCache auctionCache)
        {
            _auctionCache = auctionCache;
            mainChannel = GrpcChannel.ForAddress($"http://localhost:{MainChannelPort}");
            broadcastClient = new BroadcastService.BroadcastServiceClient(mainChannel);
        }

        public override Task<PlaceBidResponse> PlaceBid(PlaceBidRequest request, ServerCallContext context)
        {
            bool bidAccepted = false /* Check if the bid is accepted */;

            var auction = _auctionCache.GetAuctions().Where(p => p.AuctionId == request.AuctionId).FirstOrDefault();

            if (auction != null)
            {
                var bidAmount = request.BidAmount;

                if(auction.AuctionRequest.StartingPrice< bidAmount)
                {
                    //sent message to the channel about new bid price
                    UpdateChannelAboutBid(request, auction, bidAmount);
                    bidAccepted = true;
                }
            }

            // Return whether the bid was accepted in the response
            return Task.FromResult(new PlaceBidResponse { Accepted = bidAccepted });
        }


        /// <summary>
        /// That function should return all the auctions. This is something slightly wrong as it returns just local auctions. 
        /// Normally it should get all the registered nodes as parameter and getl all the auctions of those nodes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<AuctionList> GetAllAuctions(AuctionEmpty request, ServerCallContext context)
        {
            var idealist = new AuctionList();
            var allNodeAuctions = _auctionCache.GetAuctions();
            idealist.AuctionList_.AddRange(allNodeAuctions);
            return Task.FromResult(idealist);
        }

        public override Task<AuctionEmpty> FinalizeAuction(AuctionResponse request, ServerCallContext context)
        {
            Console.WriteLine("Congrats..You re the winner. Bid is closed");

            UpdateChannelAboutAuctionEnded(request);
            // update all nodes
            return Task.FromResult(new AuctionEmpty());
        }

        private void UpdateChannelAboutAuctionEnded(AuctionResponse request)
        {


            var broadCastRequest = new BroadcastMessage
            {
                Text = "delete",
                AuctionId = request.AuctionId,
                OwnerId = request.OwnerNodeId,
                Address = request.Address,
                ItemName = request.AuctionRequest.ItemName,
                StartingPrice = request.AuctionRequest.StartingPrice,
                Bidder = request.Bidder
            };
            _ = broadcastClient?.Broadcast(broadCastRequest);
        }

        private void UpdateChannelAboutBid(PlaceBidRequest request, AuctionResponse? auction, double bidAmount)
        {
            var broadCastRequest = new BroadcastMessage
            {
                Text = "update",
                AuctionId = auction.AuctionId,
                OwnerId = auction.OwnerNodeId,
                Address = auction.Address,
                ItemName = auction.AuctionRequest.ItemName,
                StartingPrice = bidAmount,
                Bidder = request.Bidder,
                BidderAddress= request.BidderAdress
            };
            _ = broadcastClient?.Broadcast(broadCastRequest);
        }
    }
}
