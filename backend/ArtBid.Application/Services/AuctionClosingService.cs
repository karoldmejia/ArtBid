using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ArtBid.Application.Repositories;
using ArtBid.Application.Interfaces;

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

                    var now = DateTime.UtcNow;
                    var activeAuctions = auctionRepo.GetActiveAuctions()
                                                    .Where(a => a.EndTime <= now)
                                                    .ToList();

                    foreach (var auction in activeAuctions)
                    {
                        auction.CloseAuction(); // cambia estado a Ended y asigna ganador
                        auctionRepo.Update(auction);

                        // Confirmar cobro del ganador
                        if (auction.WinnerId.HasValue)
                        {
                            var winner = userRepo.GetById(auction.WinnerId.Value);
                            winner.ConfirmCharge(auction.CurrentPrice);
                            userRepo.Update(winner);
                            
                            // Notificar a través del notifier
                            await _auctionNotifier.NotifyAuctionClosed(auction.Id, auction.WinnerId.Value, auction.CurrentPrice);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en AuctionClosingService: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}