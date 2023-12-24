

namespace Shared.Services
{
    public class AuctionCache : IAuctionCache
    {
        private readonly Dictionary<string, AuctionResponse> _auctions = new();

        public void AddAuction(AuctionResponse auction)
        {
            if (auction == null)
            {
                throw new ArgumentNullException(nameof(auction));
            }

            auction.AuctionId = Guid.NewGuid().ToString();
            _auctions[auction.AuctionId] = auction;
        }

        public void DeleteAuction(AuctionResponse auction)
        {
            if (_auctions.Remove(auction.AuctionId))
            {
                Console.WriteLine($"Auction with ID {auction.AuctionId} deleted.");
            }
        }

        public List<AuctionResponse> GetMyAuctions()
        {
            return _auctions.Values.ToList();
        }

        public void UpdateAuction(AuctionResponse auction)
        {
            if (auction == null)
            {
                throw new ArgumentNullException(nameof(auction));
            }

            if (_auctions.ContainsKey(auction.AuctionId))
            {
                _auctions[auction.AuctionId] = auction;
            }
            else
            {
                throw new KeyNotFoundException($"Auction with ID {auction.AuctionId} not found.");
            }
        }
    }
}
