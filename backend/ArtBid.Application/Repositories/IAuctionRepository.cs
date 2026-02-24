using ArtBid.Domain.Entities;

namespace ArtBid.Application.Repositories
{
    public interface IAuctionRepository
    {
        void Add(Auction auction);
        Auction GetById(Guid auctionId);
        IEnumerable<Auction> GetActiveAuctions();
        IEnumerable<Auction> GetAuctions();
        void Update(Auction auction);

        void AddParticipant(AuctionParticipant participant);
        bool IsParticipant(Guid auctionId, Guid userId);
        IEnumerable<object> GetParticipantsByAuction(Guid auctionId);
        IEnumerable<Auction> GetAuctionsByUser(Guid userId);
        IEnumerable<Auction> GetAuctionsBySeller(Guid sellerId);
    }
}