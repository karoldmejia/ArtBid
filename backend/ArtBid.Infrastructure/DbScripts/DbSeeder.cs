using ArtBid.Domain.Entities;
using ArtBid.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static void Seed(AuctionDbContext dbContext)
    {
        dbContext.Bids.RemoveRange(dbContext.Bids);
        dbContext.Auctions.RemoveRange(dbContext.Auctions);
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.SaveChanges();

        // Crear 15 usuarios demo
        var users = new List<User>();
        for (int i = 1; i <= 15; i++)
        {
            var user = new User(
                username: $"User{i}",
                email: $"user{i}@demo.com",
                passwordHash: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"pass{i}")),
                initialBalance: 1000m
            );
            users.Add(user);
        }
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        // Crear 3 subastas demo
        var now = DateTime.UtcNow;
        var auctions = new List<Auction>
        {
            new Auction(
                title: "Subasta 1",
                description: "Es una pintura al óleo sobre lienzo con unas dimensiones de 81 centímetros de alto por 65 cm de ancho. ",
                photo: "https://commons.wikimedia.org/wiki/File:Fragonard,_The_Swing.jpg",
                artworkName: "The Swing",
                artworkAuthor: "Jean-Honoré Fragonard",
                startingPrice: 100,
                startTime: now,
                endTime: now.AddMinutes(30),
                sellerId: users[0].Id
            ),
            new Auction(
                title: "Subasta 2",
                description: "El lienzo muestra un ritual de aquelarre, presidido por el gran macho cabrío, una de las formas que toma el demonio, en el centro de la composición. A su alrededor aparecen brujas ancianas y jóvenes que le dan niños con los que, según la superstición de la época, se alimentaba.",
                photo: "https://commons.wikimedia.org/wiki/File:GOYA_-_El_aquelarre_(Museo_L%C3%A1zaro_Galdiano,_Madrid,_1797-98).jpg",
                artworkName: "El Aquelarre",
                artworkAuthor: "Francisco de Goya",
                startingPrice: 150,
                startTime: now,
                endTime: now.AddMinutes(45),
                sellerId: users[1].Id
            ),
            new Auction(
                title: "Subasta 3",
                description: "Las dos Fridas es un doble autorretrato en el que dos mujeres comparten el mismo asiento y sus rostros duplicados se muestran inexpresivos.",
                photo: "https://artsdot.com/media/artworks/images/full/71/24/7124ac21af18405d9ffe3b117d64fbfb.Jpg",
                artworkName: "Las dos Fridas",
                artworkAuthor: "Frida Kahlo",
                startingPrice: 200,
                startTime: now,
                endTime: now.AddMinutes(60),
                sellerId: users[2].Id
            )
        };
        dbContext.Auctions.AddRange(auctions);
        dbContext.SaveChanges();
    }
}