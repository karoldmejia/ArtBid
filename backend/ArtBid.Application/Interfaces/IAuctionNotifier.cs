namespace ArtBid.Application.Interfaces
{
    public interface IAuctionNotifier
    {
        Task NotifyBidPlaced(Guid auctionId, decimal amount, Guid userId);
        Task NotifyAuctionClosed(Guid auctionId, Guid winnerId, decimal finalPrice);
    }
}