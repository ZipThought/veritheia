using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// User management service - MVP 4.1
/// Post-DDD: Direct EF Core usage
/// </summary>
public class UserService
{
    private readonly VeritheiaDbContext _db;
    private readonly ILogger<UserService> _logger;
    
    public UserService(VeritheiaDbContext dbContext, ILogger<UserService> logger)
    {
        _db = dbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Create new user with default persona
    /// </summary>
    public async Task<User> CreateUserAsync(string name, string email)
    {
        // Check if user exists
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
        {
            _logger.LogWarning("User with email {Email} already exists", email);
            return existing;
        }
        
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            DisplayName = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Users.Add(user);
        
        // Create default persona
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "General",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Personas.Add(persona);
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Created user {UserId} with default persona", user.Id);
        
        return user;
    }
    
    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db.Users
            .Include(u => u.Personas)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    /// <summary>
    /// Get user with full context
    /// </summary>
    public async Task<User?> GetUserWithContextAsync(Guid userId)
    {
        return await _db.Users
            .Include(u => u.Personas)
            .Include(u => u.Journeys)
                .ThenInclude(j => j.Persona)
            .Include(u => u.Documents)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    /// <summary>
    /// Get or create default user for single-user MVP
    /// </summary>
    public async Task<User> GetOrCreateDefaultUserAsync()
    {
        var defaultEmail = "user@veritheia.local";
        var user = await GetUserByEmailAsync(defaultEmail);
        
        if (user == null)
        {
            user = await CreateUserAsync("Default User", defaultEmail);
            _logger.LogInformation("Created default user for single-user MVP");
        }
        
        return user;
    }
    
    /// <summary>
    /// Update user profile
    /// </summary>
    public async Task UpdateUserProfileAsync(Guid userId, string name, string email)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");
        
        user.DisplayName = name;
        user.Email = email;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Get user statistics
    /// </summary>
    public async Task<UserStatistics> GetUserStatisticsAsync(Guid userId)
    {
        var stats = new UserStatistics
        {
            TotalJourneys = await _db.Journeys.CountAsync(j => j.UserId == userId),
            ActiveJourneys = await _db.Journeys.CountAsync(j => j.UserId == userId && j.State == "Active"),
            TotalDocuments = await _db.Documents.CountAsync(d => d.UserId == userId),
            TotalPersonas = await _db.Personas.CountAsync(p => p.UserId == userId),
            JournalEntries = await _db.JournalEntries
                .Include(e => e.Journal)
                    .ThenInclude(j => j.Journey)
                .CountAsync(e => e.Journal.Journey.UserId == userId)
        };
        
        return stats;
    }
}

public class UserStatistics
{
    public int TotalJourneys { get; set; }
    public int ActiveJourneys { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalPersonas { get; set; }
    public int JournalEntries { get; set; }
}