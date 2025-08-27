using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Veritheia.Data.Entities;
using Veritheia.ApiService.Services;
using Veritheia.Common.Models;

namespace veritheia.Web.Services;

/// <summary>
/// Authentication service implementing Pattern A: Simple Identifier Authentication
/// </summary>
public class AuthenticationService : IAuthenticationProvider
{
    private readonly Veritheia.ApiService.Services.UserApiService _userApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(Veritheia.ApiService.Services.UserApiService userApiService, IHttpContextAccessor httpContextAccessor)
    {
        _userApiService = userApiService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Authenticate user with identifier (email) only
    /// </summary>
    public async Task<UserIdentity> AuthenticateAsync(AuthenticationRequest request)
    {
        // Create or get user from API (display name is optional)
        var displayName = request.AdditionalData?.GetValueOrDefault("displayName")?.ToString();
        var user = await _userApiService.CreateOrGetUserAsync(request.Identifier, displayName);

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
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return new UserIdentity
        {
            Id = user.Id,
            Identifier = user.Email,
            DisplayName = user.DisplayName,
            Claims = claims.ToDictionary(c => c.Type, c => c.Value)
        };
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    public async Task<UserIdentity?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var user = await _userApiService.GetUserAsync(userId);
        if (user == null)
            return null;

        return new UserIdentity
        {
            Id = user.Id,
            Identifier = user.Email,
            DisplayName = user.DisplayName,
            Claims = httpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value)
        };
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
        return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
    }

    // Legacy method for backward compatibility - will be removed in future version
    [Obsolete("Use AuthenticateAsync(AuthenticationRequest) instead")]
    public async Task<bool> LoginAsync(string email, string displayName)
    {
        var request = new AuthenticationRequest
        {
            Identifier = email,
            AdditionalData = new Dictionary<string, object> { ["displayName"] = displayName }
        };

        await AuthenticateAsync(request);
        return true;
    }
}
