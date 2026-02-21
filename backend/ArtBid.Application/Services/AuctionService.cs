using ArtBid.Application.Interfaces;
using ArtBid.Application.Repositories;
using ArtBid.Domain.Entities;

namespace ArtBid.Application.Services
{
    public class AuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuctionNotifier _auctionNotifier;

        public AuctionService(
            IAuctionRepository auctionRepository,
            IUserRepository userRepository,
            IAuctionNotifier auctionNotifier)
        {
            _auctionRepository = auctionRepository;
            _userRepository = userRepository;
            _auctionNotifier = auctionNotifier;
        }

        public Bid PlaceBid(Guid userId, Guid auctionId, decimal amount)
        {
            var auction = _auctionRepository.GetById(auctionId);
            if (auction == null)
                throw new InvalidOperationException("Auction not found");

            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            auction.PlaceBid(user, amount);
            _auctionRepository.Update(auction);
            _userRepository.Update(user);

            var bid = auction.Bids.Last();
            
            // Notificar a los clientes
            _auctionNotifier.NotifyBidPlaced(auctionId, amount, userId);

            return bid;
        }

        public IEnumerable<Auction> GetActiveAuctions()
        {
            return _auctionRepository.GetActiveAuctions();
        }

        public Auction GetById(Guid auctionId)
        {
            return _auctionRepository.GetById(auctionId);
        }

        public IEnumerable<object> GetParticipants(Guid auctionId, Guid requestingUserId)
{
    // Verificar que el usuario que pide la info ha participado
    if (!_auctionRepository.IsParticipant(auctionId, requestingUserId))
        throw new InvalidOperationException("Usuario no autorizado para ver participantes");

    // Traer los participantes desde el repositorio
    var participants = _auctionRepository.GetParticipantsByAuction(auctionId);
    return participants;
}
    }
}