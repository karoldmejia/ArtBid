using ArtBid.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtBid.Infrastructure.Persistence
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la entidad User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Balance).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configuración de Auction
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title).IsRequired();
                entity.Property(a => a.CurrentPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(a => a.StartingPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(a => a.RowVersion).IsRowVersion(); // Concurrency token
                entity.HasMany(a => a.Bids).WithOne().HasForeignKey(b => b.AuctionId);
            });

            // Configuración Bid
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Amount).HasColumnType("decimal(18,2)").IsRequired();
            });

            modelBuilder.Entity<AuctionParticipant>(entity =>
{
    entity.HasKey(ap => new { ap.AuctionId, ap.UserId });
    entity.Property(ap => ap.JoinedAt).IsRequired();
});
        }
    }
}