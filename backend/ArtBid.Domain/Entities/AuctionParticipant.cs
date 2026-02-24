namespace ArtBid.Domain.Entities

{
    public class AuctionParticipant
    {
        public Guid AuctionId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime JoinedAt { get; private set; }

        public AuctionParticipant(Guid auctionId, Guid userId)
        {
            AuctionId = auctionId;
            UserId = userId;
            JoinedAt = DateTime.Now;
        }

        protected AuctionParticipant() { }
    }
}