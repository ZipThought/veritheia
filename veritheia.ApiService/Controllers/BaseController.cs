using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Base controller for internal API service with user context extraction
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Get the current user ID from request headers (internal service communication)
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Get the current user email from request headers (internal service communication)
    /// </summary>
    protected string? GetCurrentUserEmail()
    {
        if (Request.Headers.TryGetValue("X-User-Email", out var userEmailHeader))
        {
            return userEmailHeader.FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Get current user context (for internal service communication)
    /// </summary>
    protected (Guid? UserId, string? UserEmail) GetCurrentUserContext()
    {
        return (GetCurrentUserId(), GetCurrentUserEmail());
    }
}
