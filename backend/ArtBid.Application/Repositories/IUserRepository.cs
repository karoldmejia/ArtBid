using ArtBid.Domain.Entities;

namespace ArtBid.Application.Repositories
{
    public interface IUserRepository
    {
        User GetById(Guid id);
        User? GetByEmail(string email);
        void Add(User user);
        void Update(User user);
        IEnumerable<User> GetAll();
    }
}