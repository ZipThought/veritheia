using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Service for managing users - the constant in the system
/// Handles database operations for User entities
/// </summary>
public class UserService
{
    private readonly VeritheiaDbContext _context;
    private readonly PersonaService _personaService;
    private readonly ILogger<UserService> _logger;

    public UserService(VeritheiaDbContext context, PersonaService personaService, ILogger<UserService> logger)
    {
        _context = context;
        _personaService = personaService;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Create or get user with optional display name
    /// </summary>
    public async Task<User> CreateOrGetUserAsync(string email, string? displayName = null)
    {
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            // Update last active
            existingUser.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser;
        }

        _logger.LogInformation("Creating new user: {Email}", email);

        // Generate default display name if not provided
        var defaultDisplayName = displayName ?? GenerateDefaultDisplayName(email);

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            DisplayName = defaultDisplayName,
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create default personas for new user
        await _personaService.CreateDefaultPersonasAsync(user.Id);

        _logger.LogInformation("Created new user {UserId} with email {Email}", user.Id, email);
        return user;
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    public async Task UpdateUserAsync(Guid userId, string? displayName = null)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            user.DisplayName = displayName;
        }

        user.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        if (user != null)
        {
            user.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generate default display name from email
    /// </summary>
    private string GenerateDefaultDisplayName(string email)
    {
        var localPart = email.Split('@')[0];
        return char.ToUpper(localPart[0]) + localPart[1..].ToLower();
    }
}