using System.Security.Claims;
using App.DTOs;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<UserModel> _userManager;
    private readonly SignInManager<UserModel> _signInManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Password is required." });
        }

        UserModel? existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
        {
            return Conflict(new { message = "Username already exists." });
        }

        UserModel newUser = new UserModel
        {
            UserName = request.Username
        };

        IdentityResult result = await _userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { nessage = "Registration failed", errors = result.Errors });
        }

        await _userManager.AddToRoleAsync(newUser, "Member");

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new Claim(ClaimTypes.Name, newUser.UserName!),
            new Claim(ClaimTypes.Role, "Member")
        };

        string token = _tokenService.GenerateAccessToken(claims);

        Response.Cookies.Append("auth", token, GetCookieOptions());

        return Ok(new AuthResponse
        {
            UserId = newUser.Id,
            Username = newUser.UserName!
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        UserModel? user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        string token = _tokenService.GenerateAccessToken(claims);

        Response.Cookies.Append("auth", token, GetCookieOptions());

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Username = user.UserName!
        });
    }

    //POST: api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        Response.Cookies.Delete("auth", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(new LogoutResponse
        {
            IsLoggedIn = false
        });
    }

    private CookieOptions GetCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,  // Set to true in production with HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        };
    }
}