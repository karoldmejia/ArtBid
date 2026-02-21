using Microsoft.AspNetCore.SignalR;
using ArtBid.Application.Services;

namespace ArtBid.API.Hubs
{
    public class AuctionHub : Hub
    {
        private readonly AuctionService _auctionService;

        public AuctionHub(AuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        // Unirse a un grupo de subasta
        public async Task JoinAuction(Guid auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
        }

        // Salir de la subasta
        public async Task LeaveAuction(Guid auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
        }

        // Colocar oferta
        public async Task PlaceBid(Guid auctionId, decimal amount)
        {
            // Obtener ID de usuario desde JWT/Claims
            var subClaim = Context.User?.FindFirst("sub")?.Value;
            if (subClaim == null || !Guid.TryParse(subClaim, out var userId))
                throw new HubException("Usuario no autenticado o inválido");

            var bid = _auctionService.PlaceBid(userId, auctionId, amount);

            // Notificar a todos los usuarios del grupo
            await Clients.Group(auctionId.ToString()).SendAsync("BidPlaced", new
            {
                bid.Id,
                bid.UserId,
                bid.Amount,
                bid.Timestamp
            });
        }
    }
}