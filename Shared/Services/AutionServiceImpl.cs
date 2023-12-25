using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public class AutionServiceImpl : AuctionService.AuctionServiceBase
    {
        private readonly IAuctionCache _auctionCache;

        public AutionServiceImpl(IAuctionCache auctionCache)
        {
            _auctionCache = auctionCache;
        }
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

            var auction = _auctionCache.GetAuctions().Where(p => p.AuctionId == request.AuctionId).FirstOrDefault();

            if (auction != null)
            {
                var bidAmount = request.BidAmount;

                if(auction.AuctionRequest.StartingPrice< bidAmount)
                {
                    auction.AuctionRequest.StartingPrice = bidAmount;

                    var newAuctionInstance = new AuctionResponse
                    {
                        Address = auction.Address,
                        AuctionId = auction.AuctionId,
                        OwnerNodeId = auction.OwnerNodeId,
                        Bidder= request.Bidder,
                        AuctionRequest = new InitiateAuctionRequest
                        {
                            ItemName = auction.AuctionRequest.ItemName,
                            StartingPrice = bidAmount,
                        }
                    };

                    _auctionCache.UpdateAuction(newAuctionInstance);
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

        public override Task<AuctionEmpty> CloseAuction(AuctionResponse request, ServerCallContext context)
        {
            _auctionCache.DeleteAuction(request);
            // update all nodes
            return Task.FromResult(new AuctionEmpty());
        }
    }
}
