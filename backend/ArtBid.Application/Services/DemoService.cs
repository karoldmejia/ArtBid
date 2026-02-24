using ArtBid.Application.Repositories;
using ArtBid.Application.Services;
using ArtBid.Domain.Entities;

public class DemoScriptService
{
    private readonly AuctionService _auctionService;
    private readonly IAuctionRepository _auctionRepo;
    private readonly IUserRepository _userRepo;

    public DemoScriptService(
        AuctionService auctionService,
        IAuctionRepository auctionRepo,
        IUserRepository userRepo)
    {
        _auctionService = auctionService;
        _auctionRepo = auctionRepo;
        _userRepo = userRepo;
    }

    private List<User> GetDemoUsers()
    {
        var user1 = _userRepo.GetAll().Cast<User>().FirstOrDefault(u => u.Username == "User1");
        var user2 = _userRepo.GetAll().Cast<User>().FirstOrDefault(u => u.Username == "User2");
        var user3 = _userRepo.GetAll().Cast<User>().First(u => u.Username == "User3");

        return new List<User> { user1, user2, user3 };
    }

    private Auction GetFirstActiveAuction()
    {
        return _auctionRepo.GetActiveAuctions().First();
    }

    public async Task RunBasicBidWar()
    {
        var auction = GetFirstActiveAuction();
        var users = GetDemoUsers();

        decimal amount = auction.CurrentPrice;

        Console.WriteLine("Iniciando guerra básica de ofertas...");
        foreach (var user in users)
        {
            amount += 50;

            _auctionService.PlaceBid(user.Id, auction.Id, amount);

            Console.WriteLine($"{user.Username} ofertó {amount}");

            await Task.Delay(2000);
        }

        Console.WriteLine("Fin de la guerra básica.");
    }

    public async Task RunMultiAuctionDemo()
    {
        var auctions = _auctionRepo.GetActiveAuctions().Take(2).ToList();
        var users = GetDemoUsers();

        Console.WriteLine("Simulando múltiples subastas activas...");

        foreach (var auction in auctions)
        {
            decimal amount = auction.CurrentPrice;

            Console.WriteLine($"Subasta: {auction.Title}");

            foreach (var user in users)
            {
                amount += 30;

                _auctionService.PlaceBid(user.Id, auction.Id, amount);

                Console.WriteLine($"{user.Username} ofertó {amount}");

                await Task.Delay(1500);
            }
        }

        Console.WriteLine("Fin de simulación multi-subasta.");
    }

    public async Task RunTieBidDemo()
    {
        var auction = GetFirstActiveAuction();
        var users = GetDemoUsers();

        var user1 = users[0];
        var user2 = users[1];
        var user3 = users[2];

        decimal targetAmount = auction.CurrentPrice + 100;

        Console.WriteLine("Simulando ofertas simultáneas...");

        var task1 = Task.Run(() =>
        {
            try
            {
                _auctionService.PlaceBid(user2.Id, auction.Id, targetAmount);
                Console.WriteLine($"{user2.Username} ofertó {targetAmount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{user2.Username} falló: {ex.Message}");
            }
        });

        var task2 = Task.Run(() =>
        {
            try
            {
                _auctionService.PlaceBid(user3.Id, auction.Id, targetAmount);
                Console.WriteLine($"{user3.Username} ofertó {targetAmount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{user3.Username} falló: {ex.Message}");
            }
        });

        await Task.WhenAll(task1, task2);

        await Task.Delay(2000);

        Console.WriteLine("Ahora el usuario 1 hará la oferta final...");

        _auctionService.PlaceBid(user1.Id, auction.Id, targetAmount + 200);

        Console.WriteLine($"{user1.Username} ganó con {targetAmount + 200}");
    }
    public async Task RunAuctionClosureDemo()
    {
        var auction = GetFirstActiveAuction();

        Console.WriteLine("Simulando cierre de subasta...");

        await Task.Delay(3000);

        auction.ForceClose();

        _auctionRepo.Update(auction);

        Console.WriteLine($"Subasta cerrada.");
        Console.WriteLine($"Ganador: {auction.WinnerId}");
    }
}