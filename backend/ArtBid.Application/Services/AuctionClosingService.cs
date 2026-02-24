using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ArtBid.Application.Repositories;
using ArtBid.Application.Interfaces;
using ArtBid.Domain.Enums;

namespace ArtBid.Application.Services
{
    public class AuctionClosingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuctionNotifier _auctionNotifier;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        public AuctionClosingService(IServiceProvider serviceProvider, IAuctionNotifier auctionNotifier)
        {
            _serviceProvider = serviceProvider;
            _auctionNotifier = auctionNotifier;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var auctionRepo = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var now = DateTime.Now;

                    var activeAuctions = auctionRepo.GetActiveAuctions()
                        .Where(a => a.EndTime <= now && a.Status == AuctionStatus.Active)
                        .ToList();

                    foreach (var auction in activeAuctions)
                    {

                        if (auction.Status != AuctionStatus.Active)
                            continue;

                        // Identificar al ganador (la oferta más alta)
                        var winnerBid = auction.Bids
                            .OrderByDescending(b => b.Amount)
                            .ThenByDescending(b => b.Timestamp)
                            .FirstOrDefault();

                        var activeBidsByUser = auction.Bids
                            .GroupBy(b => b.UserId)
                            .Select(g => g.OrderByDescending(b => b.Amount).First()) // La de mayor monto
                            .ToList();

                        foreach (var bid in activeBidsByUser)
                        {
                            var bidder = userRepo.GetById(bid.UserId);

                            if (winnerBid != null && bid.UserId == winnerBid.UserId)
                            {
                                userRepo.Update(bidder);
                            }
                        }
                        // Cerrar la subasta
                        auction.CloseAuction();
                        auctionRepo.Update(auction);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AuctionClosingService] Error: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}