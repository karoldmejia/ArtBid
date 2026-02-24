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
        public decimal CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; }
        public Guid SellerId { get; set; }
        public Guid? WinnerId { get; set; }

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

        public void AddBid(Bid bid)
        {
            _bids.Add(bid);
        }

        public void PlaceBid(User user, decimal amount)
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction not active");

            if (DateTime.Now > EndTime)
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

            var winnerBid = _bids.OrderByDescending(b => b.Amount)
                                 .ThenBy(b => b.Timestamp)
                                 .FirstOrDefault();

            if (winnerBid != null)
            {
                WinnerId = winnerBid.UserId;
            }
        }

        public void ForceClose()
        {
            if (Status == AuctionStatus.Ended || Status == AuctionStatus.Cancelled)
                throw new InvalidOperationException("Auction is already closed or cancelled");

            Status = AuctionStatus.Ended;

            if (_bids.Any())
            {
                var winnerBid = _bids.OrderByDescending(b => b.Amount)
                                     .ThenBy(b => b.Timestamp)
                                     .FirstOrDefault();
                if (winnerBid != null)
                {
                    WinnerId = winnerBid.UserId;
                }
            }
            else
            {
                // Si no hay bids, no hay ganador
                WinnerId = null;
            }
        }
    }
}