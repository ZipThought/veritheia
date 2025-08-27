using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Veritheia.Common.Models;

namespace veritheia.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly veritheia.Web.Services.AuthenticationService _authService;
    private readonly Veritheia.ApiService.Services.UserApiService _userApiService;

    public AuthController(veritheia.Web.Services.AuthenticationService authService, Veritheia.ApiService.Services.UserApiService userApiService)
    {
        _authService = authService;
        _userApiService = userApiService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        try
        {
            // Create or get user
            var user = await _userApiService.CreateOrGetUserAsync(email.Trim(), null);
            
            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Sign in the user - this works in a controller because response hasn't started
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect to home page after successful login
            return Redirect("/");
        }
        catch (Exception ex)
        {
            return BadRequest($"Authentication failed: {ex.Message}");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}