using ArtBid.Application.Repositories;
using ArtBid.Domain.Entities;
using ArtBid.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArtBid.Infrastructure.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _dbContext;

        public AuctionRepository(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Auction auction)
        {
            _dbContext.Auctions.Add(auction);
            _dbContext.SaveChanges();
        }

        public Auction GetById(Guid auctionId)
        {
            var auction = _dbContext.Auctions
                .Include(a => a.Bids)
                .FirstOrDefault(a => a.Id == auctionId);

            return auction ?? throw new InvalidOperationException($"Auction with ID '{auctionId}' not found.");
        }

        public IEnumerable<Auction> GetActiveAuctions()
        {
            return _dbContext.Auctions
                .Where(a => a.Status == Domain.Enums.AuctionStatus.Active)
                .Include(a => a.Bids)
                .ToList();
        }

        public IEnumerable<Auction> GetAuctionsBySeller(Guid sellerId)
        {
            return _dbContext.Auctions
                .Include(a => a.Bids)
                .Where(a => a.SellerId == sellerId)
                .ToList();
        }

        public IEnumerable<Auction> GetAuctionsByUser(Guid userId)
        {
            return _dbContext.Auctions
                .Include(a => a.Bids)
                .Where(a => a.Bids.Any(b => b.UserId == userId))
                .ToList();
        }

        public void Update(Auction auction)
        {
            _dbContext.Auctions.Update(auction);
            _dbContext.SaveChanges();
        }

        // AuctionParticipant
        public void AddParticipant(AuctionParticipant participant)
        {
            if (!IsParticipant(participant.AuctionId, participant.UserId))
            {
                _dbContext.Set<AuctionParticipant>().Add(participant);
                _dbContext.SaveChanges();
            }
        }

        public bool IsParticipant(Guid auctionId, Guid userId)
        {
            return _dbContext.Set<AuctionParticipant>()
                .Any(ap => ap.AuctionId == auctionId && ap.UserId == userId);
        }

        public IEnumerable<object> GetParticipantsByAuction(Guid auctionId)
        {
            return _dbContext.Bids
                .Where(b => b.AuctionId == auctionId)
                .Join(_dbContext.Users,
                      b => b.UserId,
                      u => u.Id,
                      (b, u) => new
                      {
                          u.Id,
                          u.Username,
                          u.Email
                      })
                .Distinct()
                .ToList();
        }
    }
}