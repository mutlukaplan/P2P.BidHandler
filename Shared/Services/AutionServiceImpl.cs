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
            _auctionCache=auctionCache;
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

            // Return whether the bid was accepted in the response
            return Task.FromResult(new PlaceBidResponse { Accepted = bidAccepted });
        }

        public override Task<AuctionList> GetAllAuctions(AuctionEmpty request, ServerCallContext context)
        {
            var idealist = new AuctionList();
            idealist.AuctionList_.Add(new AuctionResponse());

            return Task.FromResult(idealist);
        }

        public override Task<AuctionEmpty> CloseAuction(AuctionResponse request, ServerCallContext context)
        {
            _auctionCache.DeleteAuction(request);

            return Task.FromResult(new AuctionEmpty());
        }
    }
}
