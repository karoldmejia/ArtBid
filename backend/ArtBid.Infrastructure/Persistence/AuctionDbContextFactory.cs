using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArtBid.Infrastructure.Persistence
{
    public class AuctionDbContextFactory : IDesignTimeDbContextFactory<AuctionDbContext>
    {
        public AuctionDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuctionDbContext>();

            optionsBuilder.UseSqlite("Data Source=artbid.db");

            return new AuctionDbContext(optionsBuilder.Options);
        }
    }
}