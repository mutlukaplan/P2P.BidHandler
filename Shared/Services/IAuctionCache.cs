using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public interface IAuctionCache
    {
        void AddAuction(AuctionResponse auction);
        void UpdateAuction(AuctionResponse auction);
        void DeleteAuction(AuctionResponse auction);
        List<AuctionResponse> GetAuctions();
    }
}
