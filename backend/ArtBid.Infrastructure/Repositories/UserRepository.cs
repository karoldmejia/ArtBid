using ArtBid.Application.Repositories;
using ArtBid.Domain.Entities;
using ArtBid.Infrastructure.Persistence;

namespace ArtBid.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuctionDbContext _context;

        public UserRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public User GetById(Guid id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id)
                ?? throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public bool ExistsByEmail(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }
    }
}