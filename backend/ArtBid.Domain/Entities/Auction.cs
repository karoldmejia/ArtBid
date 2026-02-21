using ArtBid.Domain.Enums;
namespace ArtBid.Domain.Entities
{
    public class Auction
    {
        public Guid Id { get; private set; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }
        public string? Photo { get; private set; }
        public string? ArtworkName { get; private set; }
        public string? ArtworkAuthor { get; private set; }
        public decimal StartingPrice { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public AuctionStatus Status { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid? WinnerId { get; private set; }

        private readonly List<Bid> _bids = new();
        public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

        public byte[]? RowVersion { get; private set; }

        public Auction(string title, string description, string photo, string artworkName,
                       string artworkAuthor, decimal startingPrice, DateTime startTime,
                       DateTime endTime, Guid sellerId)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            Photo = photo;
            ArtworkName = artworkName;
            ArtworkAuthor = artworkAuthor;
            StartingPrice = startingPrice;
            CurrentPrice = startingPrice;
            StartTime = startTime;
            EndTime = endTime;
            Status = AuctionStatus.Active;
            SellerId = sellerId;
        }

        protected Auction() { } 

        public void PlaceBid(User user, decimal amount)
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction not active");

            if (DateTime.UtcNow > EndTime)
                throw new InvalidOperationException("Auction ended");

            if (amount <= CurrentPrice)
                throw new InvalidOperationException("Bid too low");

            user.Reserve(amount);

            CurrentPrice = amount;
            _bids.Add(new Bid(user.Id, Id, amount));
        }

        public void CloseAuction()
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction already closed");

            Status = AuctionStatus.Ended;

            var winnerBid = _bids.OrderByDescending(b => b.Amount).ThenBy(b => b.Timestamp).FirstOrDefault();
            if (winnerBid != null)
            {
                WinnerId = winnerBid.UserId;
            }
        }
    }
}