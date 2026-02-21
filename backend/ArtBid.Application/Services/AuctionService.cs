using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ArtBid.Application.Repositories;
using ArtBid.Domain.Entities;
using ArtBid.Application.Interfaces;

namespace ArtBid.Application.Services
{

    public class AuctionClosingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuctionNotifier _notifier;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        public AuctionClosingService(IServiceProvider serviceProvider, IAuctionNotifier notifier)
        {
            _serviceProvider = serviceProvider;
            _notifier = notifier;
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
                        auction.CloseAuction();
                        auctionRepo.Update(auction);

                        if (auction.WinnerId.HasValue)
                        {
                            var winner = userRepo.GetById(auction.WinnerId.Value);
                            winner.ConfirmCharge(auction.CurrentPrice);
                            userRepo.Update(winner);
                        }

                        // Llamamos al notifier genérico
                        await _notifier.NotifyAuctionClosed(auction.Id, auction.Title, auction.WinnerId, auction.CurrentPrice);
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