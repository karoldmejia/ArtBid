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
            try
            {

                var auction = _auctionRepository.GetById(auctionId);
                if (auction == null)
                    throw new InvalidOperationException("Auction not found");

                var user = _userRepository.GetById(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                if (DateTime.Now > auction.EndTime)
                    throw new InvalidOperationException("Auction ended");

                if (amount <= auction.CurrentPrice)
                    throw new InvalidOperationException("Bid too low");

                // Obtener el actual mejor postor
                var currentWinnerBid = auction.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                // Si hay un ganador actual y es diferente al nuevo postor
                if (currentWinnerBid != null && currentWinnerBid.UserId != userId)
                {
                    var currentWinner = _userRepository.GetById(currentWinnerBid.UserId);
                    currentWinner.Release(currentWinnerBid.Amount);
                    _userRepository.Update(currentWinner);
                }

                // Reservar del nuevo postor
                user.Reserve(amount);

                // Actualizar la subasta
                auction.CurrentPrice = amount;
                auction.AddBid(new Bid(user.Id, auction.Id, amount));

                // Guardar cambios
                _auctionRepository.Update(auction);
                _userRepository.Update(user);

                var bid = auction.Bids.Last();

                // Notificar
                _auctionNotifier.NotifyBidPlaced(auctionId, amount, userId);

                return bid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PlaceBid: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Auction> GetActiveAuctions()
        {
            return _auctionRepository.GetActiveAuctions();
        }

        public IEnumerable<Auction> GetAuctions()
        {
            return _auctionRepository.GetAuctions();
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