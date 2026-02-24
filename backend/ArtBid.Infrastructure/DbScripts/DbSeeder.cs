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
        var now = DateTime.Now;
        var auctions = new List<Auction>
        {
            new Auction(
                title: "Subasta 1",
                description: "Es una pintura al óleo sobre lienzo con unas dimensiones de 81 centímetros de alto por 65 cm de ancho. ",
                photo: "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/The_Swing_%28P430%29.jpg/960px-The_Swing_%28P430%29.jpg",
                artworkName: "The Swing",
                artworkAuthor: "Jean-Honoré Fragonard",
                startingPrice: 100,
                startTime: now,
                endTime: now.AddMinutes(3),
                sellerId: users[5].Id
            ),
            new Auction(
                title: "Subasta 2",
                description: "El lienzo muestra un ritual de aquelarre, presidido por el gran macho cabrío, una de las formas que toma el demonio, en el centro de la composición. A su alrededor aparecen brujas ancianas y jóvenes que le dan niños con los que, según la superstición de la época, se alimentaba.",
                photo: "https://upload.wikimedia.org/wikipedia/commons/7/74/GOYA_-_El_aquelarre_%28Museo_L%C3%A1zaro_Galdiano%2C_Madrid%2C_1797-98%29.jpg",
                artworkName: "El Aquelarre",
                artworkAuthor: "Francisco de Goya",
                startingPrice: 150,
                startTime: now,
                endTime: now.AddMinutes(2),
                sellerId: users[6].Id
            ),
            new Auction(
                title: "Subasta 3",
                description: "Las dos Fridas es un doble autorretrato en el que dos mujeres comparten el mismo asiento y sus rostros duplicados se muestran inexpresivos.",
                photo: "https://artsdot.com/media/artworks/images/full/71/24/7124ac21af18405d9ffe3b117d64fbfb.Jpg",
                artworkName: "Las dos Fridas",
                artworkAuthor: "Frida Kahlo",
                startingPrice: 200,
                startTime: now,
                endTime: now.AddMinutes(4),
                sellerId: users[0].Id
            )
        };
        dbContext.Auctions.AddRange(auctions);
        dbContext.SaveChanges();
    }
}