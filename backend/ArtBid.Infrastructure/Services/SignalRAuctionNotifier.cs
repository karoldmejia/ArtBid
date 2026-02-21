using Microsoft.AspNetCore.SignalR;
using ArtBid.Application.Interfaces;
using ArtBid.API.Hubs;

namespace ArtBid.Infrastructure.Services
{
    public class SignalRAuctionNotifier : IAuctionNotifier
    {
        private readonly IHubContext<AuctionHub> _hubContext;

        public SignalRAuctionNotifier(IHubContext<AuctionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyBidPlaced(Guid auctionId, decimal amount, Guid userId)
        {
            await _hubContext.Clients.Group(auctionId.ToString())
                       .SendAsync("BidPlaced", new 
                       { 
                           AuctionId = auctionId, 
                           Amount = amount, 
                           UserId = userId,
                           Timestamp = DateTime.UtcNow
                       });
        }

        public async Task NotifyAuctionClosed(Guid auctionId, Guid winnerId, decimal finalPrice)
        {
            await _hubContext.Clients.Group(auctionId.ToString())
                       .SendAsync("AuctionClosed", new 
                       { 
                           AuctionId = auctionId, 
                           WinnerId = winnerId,
                           FinalPrice = finalPrice
                       });
        }
    }
}