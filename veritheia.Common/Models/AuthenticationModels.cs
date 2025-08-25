namespace Veritheia.Common.Models;

/// <summary>
/// Authentication provider interface as defined in specification
/// </summary>
public interface IAuthenticationProvider
{
    Task<UserIdentity> AuthenticateAsync(AuthenticationRequest request);
    Task<UserIdentity?> GetCurrentUserAsync();
    Task LogoutAsync();
    bool IsAuthenticated();
}

/// <summary>
/// User manager interface as defined in specification
/// </summary>
public interface IUserManager
{
    Task<UserIdentity> CreateOrGetUserAsync(string identifier, string? displayName = null);
    Task<UserIdentity?> GetUserAsync(Guid userId);
    Task<UserIdentity?> GetUserByIdentifierAsync(string identifier);
    Task UpdateUserAsync(Guid userId, UserUpdateRequest request);
}

/// <summary>
/// User identity as defined in specification
/// </summary>
public class UserIdentity
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty; // Email, username, or external ID
    public string? DisplayName { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

/// <summary>
/// Authentication request as defined in specification
/// </summary>
public class AuthenticationRequest
{
    public string Identifier { get; set; } = string.Empty; // Email, username, or external ID
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// User update request as defined in specification
/// </summary>
public class UserUpdateRequest
{
    public string? DisplayName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Authentication exception for error handling
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
}
