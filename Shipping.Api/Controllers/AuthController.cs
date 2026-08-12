using Microsoft.AspNetCore.Mvc;
using ShipCore.DTOs;
using ShipCore.Services;

namespace ShipCore.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);

        if (user is null)
        {
            return BadRequest(new
            {
                message = "Email already exists"
            });
        }

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }

        [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password"
            });
        }

        return Ok(new
        {
            token
        });
    }
}