using ArtBid.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

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

    // Perfil del usuario con balance y subastas participadas
    [HttpGet("me")]
    public IActionResult GetProfile()
    {
        var subClaim = User.FindFirst("sub")?.Value;
        if (subClaim == null || !Guid.TryParse(subClaim, out var userId))
            return Unauthorized("User ID not found");

        try
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
                return NotFound("User not found");

            var auctionsParticipated = _auctionRepo.GetAuctionsByUser(userId)
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
        var subClaim = User.FindFirst("sub")?.Value;
        if (subClaim == null || !Guid.TryParse(subClaim, out var userId))
            return Unauthorized("User ID not found");

        try
        {
            var auctions = _auctionRepo.GetAuctionsByUser(userId) // Devuelve todas las subastas donde el usuario hizo al menos una oferta
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
        var subClaim = User.FindFirst("sub")?.Value;
        if (subClaim == null || !Guid.TryParse(subClaim, out var userId))
            return Unauthorized("User ID not found");

        try
        {
            var auctions = _auctionRepo.GetAuctionsBySeller(userId) // Devuelve todas las subastas publicadas
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
        var subClaim = User.FindFirst("sub")?.Value;
        if (subClaim == null || !Guid.TryParse(subClaim, out var userId))
            return Unauthorized("User ID not found");

        try
        {
            var user = _userRepo.GetById(userId);
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