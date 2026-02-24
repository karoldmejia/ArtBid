namespace ArtBid.Domain.Entities
{
    public class Bid
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid AuctionId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Bid(Guid userId, Guid auctionId, decimal amount)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            AuctionId = auctionId;
            Amount = amount;
            Timestamp = DateTime.Now;
        }

        protected Bid() {}

    }
}