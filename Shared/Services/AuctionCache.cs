namespace Shared.Services
{
    public class AuctionCache : IAuctionCache
    {
        public  readonly Dictionary<string, AuctionResponse> _auctions;
        public AuctionCache(Dictionary<string, AuctionResponse> auctionCsh)
        {
            _auctions = auctionCsh;
        }

        public void AddAuction(AuctionResponse auction)
        {
            if (auction == null)
            {
                throw new ArgumentNullException(nameof(auction));
            }
            var message = string.Empty;

            if (_auctions.TryAdd(auction.AuctionId, auction))
            {
                message = $"Auction is added with the Id of {auction.AuctionId}";
                Console.WriteLine(message);
            }
            else
            {
                message = $"Node '{auction.AuctionId}' is already registered.";
                Console.WriteLine(message);
            }
        }

        public void DeleteAuction(AuctionResponse auction)
        {
            if (_auctions.Remove(auction.AuctionId))
            {
                Console.WriteLine($"Auction with ID {auction.AuctionId} deleted.");
            }
        }

        public List<AuctionResponse> GetAuctions()
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
