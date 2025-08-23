namespace Veritheia.Data.DTOs;

/// <summary>
/// Data Transfer Object for User entity
/// Used for API communication between services
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating users
/// </summary>
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
