using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Veritheia.Data.DTOs;

namespace veritheia.Web.Services;

/// <summary>
/// Authentication service for managing user sessions
/// </summary>
public class AuthenticationService
{
    private readonly UserApiService _userApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(UserApiService userApiService, IHttpContextAccessor httpContextAccessor)
    {
        _userApiService = userApiService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return await _userApiService.GetUserAsync(userId);
    }

    /// <summary>
    /// Login user with email and display name
    /// </summary>
    public async Task<bool> LoginAsync(string email, string displayName)
    {
        try
        {
            // Create or get user from API
            var user = await _userApiService.CreateOrGetUserAsync(email, displayName);
            
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

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }
}
