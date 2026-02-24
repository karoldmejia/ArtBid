using ArtBid.Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("artbid/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IAuctionRepository _auctionRepo;

    public UserController(IUserRepository userRepo, IAuctionRepository auctionRepo)
    {
        _userRepo = userRepo;
        _auctionRepo = auctionRepo;
    }

    // Método auxiliar para extraer el userId de forma consistente
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
            
        return userId;
    }

    // Perfil del usuario con balance y subastas participadas
    [HttpGet("me")]
    public IActionResult GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { error = "User ID claim not found" });

        try
        {
            var user = _userRepo.GetById(userId.Value);
            if (user == null)
                return NotFound("User not found");

            var auctionsParticipated = _auctionRepo.GetAuctionsByUser(userId.Value)
                                                   .Select(a => new
                                                   {
                                                       a.Id,
                                                       a.Title,
                                                       a.CurrentPrice,
                                                       a.Status,
                                                       BidCount = a.Bids.Count
                                                   });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Balance,
                AuctionsParticipated = auctionsParticipated
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("me/participated")]
    public IActionResult GetParticipatedAuctions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { error = "User ID claim not found" });

        try
        {
            var auctions = _auctionRepo.GetAuctionsByUser(userId.Value)
                                       .Select(a => new
                                       {
                                           a.Id,
                                           a.Title,
                                           a.Photo,
                                           a.ArtworkName,
                                           a.CurrentPrice,
                                           a.Status,
                                           EndTime = a.EndTime,
                                           BidCount = a.Bids.Count
                                       });

            return Ok(auctions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("me/published")]
    public IActionResult GetPublishedAuctions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { error = "User ID claim not found" });

        try
        {
            var auctions = _auctionRepo.GetAuctionsBySeller(userId.Value)
                                       .Select(a => new
                                       {
                                           a.Id,
                                           a.Title,
                                           a.Photo,
                                           a.ArtworkName,
                                           a.CurrentPrice,
                                           a.Status,
                                           EndTime = a.EndTime,
                                           BidCount = a.Bids.Count
                                       });

            return Ok(auctions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    [HttpGet("me/balance")]
    public IActionResult GetBalance()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { error = "User ID claim not found" });

        try
        {
            var user = _userRepo.GetById(userId.Value);
            if (user == null)
                return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Balance
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}