using ArtBid.Application.DTOs;
using ArtBid.Application.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("artbid/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpRequest request)
    {
        var response = _authService.SignUp(request);
        return Ok(response);
    }

    [HttpPost("signin")]
    public IActionResult SignIn([FromBody] SignInRequest request)
    {
        var response = _authService.SignIn(request);
        return Ok(response);
    }
}