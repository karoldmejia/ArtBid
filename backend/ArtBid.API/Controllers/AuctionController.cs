using Microsoft.AspNetCore.Mvc;
using ArtBid.Application.Services;
using System.Security.Claims;

[ApiController]
[Route("artbid/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionService _auctionService;

    public AuctionController(AuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    // Obtener todas las subastas activas
    [HttpGet("active")]
    public IActionResult GetActive()
    {
        try
        {
            var auctions = _auctionService.GetAuctions()
                                          .Select(a => new
                                          {
                                              a.Id,
                                              a.Title,
                                              a.Photo,
                                              a.ArtworkName,
                                              a.ArtworkAuthor,
                                              a.CurrentPrice,
                                              a.EndTime,
                                              BidCount = a.Bids.Count
                                          });
            return Ok(auctions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }


    // Detalle de una subasta específica, con participantes y ofertas
[HttpGet("{auctionId}")]
public IActionResult GetAuctionDetail(Guid auctionId)
{
    try
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User ID claim not found" });
        }

        var auction = _auctionService.GetById(auctionId);
        if (auction == null)
            return NotFound("Auction not found");

        IEnumerable<object> participants;
        try
        {
            participants = _auctionService.GetParticipants(auctionId, userId);
        }
        catch (InvalidOperationException)
        {
            participants = new List<object>();
        }

        var bids = auction.Bids.Select(b => new
        {
            b.UserId,
            b.Amount,
            b.Timestamp
        });

        return Ok(new
        {
            auction.Id,
            auction.Title,
            auction.Description,
            auction.Photo,
            auction.ArtworkName,
            auction.ArtworkAuthor,
            auction.CurrentPrice,
            auction.StartingPrice,
            auction.StartTime,
            auction.EndTime,
            auction.Status,
            auction.WinnerId,
            Participants = participants,
            Bids = bids
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error: {ex.Message}");
    }
}

    // Colocar oferta vía REST
    [HttpPost("{auctionId}/bid")]
    public IActionResult PlaceBid(Guid auctionId, [FromBody] decimal amount)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized("User ID claim not found");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user ID format");

        try
        {
            var bid = _auctionService.PlaceBid(userId, auctionId, amount);
            return Ok(bid);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}