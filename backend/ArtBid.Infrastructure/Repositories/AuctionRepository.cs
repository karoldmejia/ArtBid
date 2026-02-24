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
                .AsNoTracking()
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

        public IEnumerable<Auction> GetAuctions()
        {
            return _dbContext.Auctions
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
            try
            {
                // Cargar la subasta existente con sus bids
                var existingAuction = _dbContext.Auctions
                    .Include(a => a.Bids)
                    .FirstOrDefault(a => a.Id == auction.Id);

                if (existingAuction == null)
                    throw new InvalidOperationException("Auction not found");

                // Actualizar propiedades
                existingAuction.CurrentPrice = auction.CurrentPrice;
                existingAuction.Status = auction.Status;
                existingAuction.WinnerId = auction.WinnerId;

                // **IMPORTANTE: Limpiar el tracker de bids para evitar conflictos**
                foreach (var bid in auction.Bids)
                {
                    // Verificar si esta bid ya existe en la BD
                    var existingBid = existingAuction.Bids.FirstOrDefault(b => b.Id == bid.Id);

                    if (existingBid == null)
                    {
                        // Es una NUEVA bid - Crear instancia y forzar estado Added                
                        var newBid = new Bid(bid.UserId, bid.AuctionId, bid.Amount);

                        // FORZAR el estado a Added ANTES de agregar a la colección
                        _dbContext.Entry(newBid).State = EntityState.Added;

                        // Agregar a la colección
                        existingAuction.AddBid(newBid);
                    }
                    else
                    {
                        // Actualizar bid existente (raro que pase)
                        _dbContext.Entry(existingBid).CurrentValues.SetValues(bid);
                    }
                }

                // Verificar estados ANTES de guardar
                var entries = _dbContext.ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    Console.WriteLine($"  {entry.Entity.GetType().Name} - {entry.State}");
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Error de concurrencia: {ex.Message}");
                throw new InvalidOperationException("La subasta fue modificada por otro usuario. Intenta nuevamente.");
            }
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