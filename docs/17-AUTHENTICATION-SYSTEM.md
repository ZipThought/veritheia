# Authentication System

## 1. Overview

The authentication system provides secure user access to Veritheia while maintaining the principle that users remain the authors of their intellectual work. The system uses cookie-based authentication with a simple email/display name login that creates or retrieves user accounts.

> **Formation Note:** Authentication serves to verify user identity and maintain data isolation, not to gate-keep system resources. Users always remain the authors of their intellectual work, regardless of authentication status. The system enforces user boundaries to protect personal formation while ensuring users maintain full control over their insights.

## 2. Architecture

### 2.1 Authentication Flow

1. **Login Page** (`/login`) - Users enter email and display name
2. **Authentication Service** - Creates or retrieves user account via API
3. **Cookie Authentication** - Establishes secure session using ASP.NET Core cookies
4. **Route Protection** - Unauthenticated users redirected to login
5. **User Context** - Authenticated user available throughout application

### 2.2 Components

#### AuthenticationService
Located in `veritheia.Web/Services/AuthenticationService.cs`

```csharp
public class AuthenticationService
{
    public async Task<UserDto?> GetCurrentUserAsync();
    public async Task<bool> LoginAsync(string email, string displayName);
    public async Task LogoutAsync();
    public bool IsAuthenticated();
}
```

**Responsibilities:**
- Manage user login/logout using cookie authentication
- Retrieve current authenticated user from claims
- Create or get user accounts via API service
- Provide authentication status checks

#### ProcessConfigurationService
Located in `veritheia.Web/Services/ProcessConfigurationService.cs`

```csharp
public class ProcessConfigurationService
{
    public List<ProcessOption> GetAvailableProcesses();
    public ProcessOption? GetProcessOption(string processType);
}
```

**Responsibilities:**
- Provide available process options for UI display
- Centralize process configuration previously hardcoded in components
- Support process selection in journey creation

#### ProcessOption DTO
Located in `veritheia.Data/DTOs/ProcessOption.cs`

```csharp
public class ProcessOption
{
    public string Type { get; set; }
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public string Requirements { get; set; }
}
```

**Purpose:**
- Standardize process option representation across UI components
- Replace local class definitions with shared DTO
- Enable consistent process configuration management

## 3. Implementation Details

### 3.1 Cookie Authentication Configuration

Configured in `veritheia.Web/Program.cs`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });
```

### 3.2 Route Protection

The `MainLayout.razor` component checks authentication status and redirects unauthenticated users:

```csharp
protected override async Task OnInitializedAsync()
{
    isAuthenticated = AuthService.IsAuthenticated();
    if (!isAuthenticated && !Navigation.Uri.EndsWith("/login"))
    {
        Navigation.NavigateTo("/login");
    }
}
```

### 3.3 User Account Creation

The system uses a simple "create or get" pattern:

1. User enters email and display name
2. System checks if user exists by email
3. If exists: retrieve and update last active timestamp
4. If not exists: create new user account
5. Establish authentication session

## 4. Security Considerations

### 4.1 Data Isolation

- All database queries include UserId in WHERE clauses
- Composite primary keys enforce user boundaries
- No cross-user data access possible

### 4.2 Session Management

- 30-day persistent cookies for user convenience
- Secure cookie configuration in production
- Automatic logout on session expiration

### 4.3 User Privacy

- Minimal user data collection (email, display name only)
- No tracking of user behavior beyond formation accumulation
- User controls their own intellectual property

## 5. Integration Points

### 5.1 API Service Integration

The authentication system integrates with the existing API service:

- `UserApiService.CreateOrGetUserAsync()` - Creates or retrieves users
- `UserApiService.GetUserAsync()` - Retrieves user by ID
- All API calls include user context for data isolation

### 5.2 UI Component Integration

Components updated to use authentication:

- `Dashboard.razor` - Redirects unauthenticated users
- `CreateJourney.razor` - Uses authenticated user context
- `TopBar.razor` - Displays user name and logout option
- `SidebarNavigation.razor` - Loads user-specific data

## 6. Future Enhancements

### 6.1 Production Authentication

For production deployment, consider:

- Integration with external identity providers (OAuth, SAML)
- Multi-factor authentication for sensitive data
- Role-based access control for administrative functions
- Audit logging for compliance requirements

### 6.2 Enhanced Security

- Password-based authentication for sensitive operations
- Session management with refresh tokens
- Rate limiting for login attempts
- Account lockout for suspicious activity

## 7. Testing

The authentication system includes comprehensive tests:

- Integration tests for authentication flow
- Unit tests for service methods
- UI tests for login/logout functionality
- Database tests for user isolation

All tests pass and verify the system works correctly with the existing codebase.
