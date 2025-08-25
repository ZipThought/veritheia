# Project Architecture

## 1. Overview

The project architecture defines a composable system where components can be combined in different configurations to serve various deployment scenarios. Each component serves a specific architectural role and provides well-defined interfaces for composition.

> **Formation Note:** Project architecture serves as the structural foundation that enables flexible composition of components while maintaining clean separation of concerns. The architecture supports different deployment scenarios through component composition rather than fixed architectural layers. This composable design ensures that the system can adapt to different user needs while maintaining the core principle that users remain the authors of their intellectual work. The ApiService component preserves user agency by centralizing business logic that enforces user boundaries and data isolation, while interface components provide access patterns that never compromise the user's intellectual sovereignty.

## 2. Composable Components

### 2.1 ApiService Component

The ApiService component serves as the core business logic foundation, providing the application programming interface for all system operations. This component contains all business logic, data access patterns, and domain services without any presentation or transport concerns.

**Important**: The "API" in ApiService refers to **Application Programming Interface**, not HTTP REST API. This component is a pure business logic library that provides programming interfaces for other components to consume through direct method calls.

**Why This Matters for Formation**: The ApiService component enforces the fundamental principle that users remain the authors of their intellectual work. By centralizing business logic and user data isolation, it ensures that all operations respect user boundaries and that formation data belongs exclusively to the user. This architectural separation prevents any interface component from compromising user agency or intellectual sovereignty.

**Responsibilities**:
- Business logic implementation
- Data access and persistence
- Domain service orchestration
- Business rule enforcement
- User data isolation

**Dependencies**: 
- Core project (shared interfaces)
- Data project (entities and context)
- Common project (shared models)

**Interface Pattern**:
```csharp
public interface IUserService
{
    Task<UserDto> CreateOrGetUserAsync(string email, string? displayName);
    Task<UserDto?> GetUserAsync(Guid userId);
    Task UpdateUserAsync(Guid userId, UserUpdateRequest request);
}
```

### 2.2 Web Component

The Web component serves as a user interface component, providing the Blazor Server presentation interface. This component imports the ApiService component and calls its programming interface directly, handling user interaction and session management.

**Why This Matters for Formation**: The Web component provides the primary interface through which users engage with their formation journey. By importing ApiService directly, it ensures that all user interactions maintain the same level of data isolation and user agency. The direct method calls eliminate any risk of network-based compromises to user data or intellectual work.

**Responsibilities**:
- User interface rendering
- User interaction handling
- Authentication and session management
- Navigation and routing
- Presentation logic

**Dependencies**:
- ApiService component (business logic)
- Common project (shared models)

**Integration Pattern**:
```csharp
public class AuthenticationService : IAuthenticationProvider
{
    private readonly UserService _userService; // Direct import from ApiService
    
    public async Task<UserIdentity> AuthenticateAsync(AuthenticationRequest request)
    {
        var user = await _userService.CreateOrGetUserAsync(request.Identifier, null);
        // Authentication logic using direct method calls
    }
}
```

### 2.3 ApiGateway Component

The ApiGateway component serves as a public interface component, providing HTTP API endpoints for external system integration. This component imports the ApiService component and exposes its programming interface through HTTP protocols.

**Why This Matters for Formation**: The ApiGateway component enables external systems to access formation data while maintaining the same user boundaries and data isolation enforced by ApiService. This allows for integration with external tools and workflows without compromising the principle that users remain the authors of their intellectual work. The component ensures that all external access respects user agency and data sovereignty.

**Responsibilities**:
- HTTP API endpoints
- Request/response handling
- External authentication
- Rate limiting and throttling
- API versioning

**Dependencies**:
- ApiService component (business logic)
- Common project (shared models)

**Extension Pattern**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService; // Direct import from ApiService
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateOrGetUserAsync(request.Email, request.DisplayName);
        return Ok(user);
    }
}
```

### 2.4 MCPGateway Component

The MCPGateway component serves as an AI agent interface component, providing Model Context Protocol endpoints for AI agent integration. This component imports the ApiService component and exposes its programming interface through MCP protocols.

**Why This Matters for Formation**: The MCPGateway component enables AI agents to assist users in their formation journey while maintaining strict boundaries that preserve user agency. AI agents can access formation data and provide assistance, but they cannot generate insights or make decisions on behalf of the user. This component ensures that AI remains an instrument for user formation rather than a replacement for user authorship.

**Responsibilities**:
- MCP protocol implementation
- AI agent authentication and authorization
- Tool and resource exposure
- Context management for AI agents
- MCP-specific request/response handling

**Dependencies**:
- ApiService component (business logic)
- Common project (shared models)

**Extension Pattern**:
```csharp
public class VeritheiaMCPGateway : IMCPServer
{
    private readonly UserService _userService; // Direct import from ApiService
    private readonly JourneyService _journeyService; // Direct import from ApiService
    
    public async Task<MCPResponse> HandleRequest(MCPRequest request)
    {
        // Expose ApiService functionality through MCP protocol
        // AI agents can access user data, journeys, documents, etc.
        return await ProcessMCPRequest(request);
    }
}
```

## 3. Composition Patterns

### 3.1 Component Composition

Components can be combined in different configurations to serve various deployment scenarios. A simple deployment might combine Web and ApiService components. A public API deployment might combine ApiGateway and ApiService components. A full deployment might include all components.

**Why This Matters for Formation**: The composable architecture ensures that formation technology can be deployed in various contexts while maintaining the core principle of user agency. Whether deployed as a simple personal tool or a comprehensive enterprise system, the same user boundaries and intellectual sovereignty are preserved. This flexibility enables formation technology to serve different user needs without compromising its fundamental purpose.

### 3.2 In-Process Communication

All communication between components occurs through direct method calls within the same process. The ApiService component defines the programming interface that other components consume. This pattern eliminates network overhead while maintaining clean architectural boundaries.

**Why This Matters for Formation**: In-process communication ensures that user formation data never leaves the secure boundary of the user's system. This eliminates the risk of network-based data breaches or unauthorized access to intellectual work. The direct method calls also ensure that all operations maintain the same level of user data isolation and agency.

### 3.3 Interface Contracts

Each component defines clear interface contracts that specify the programming interface for composition. These contracts remain stable across component boundaries and enable clean separation of concerns.

**Why This Matters for Formation**: Stable interface contracts ensure that user formation data and business logic remain consistent regardless of how the system is deployed or extended. This stability preserves the user's intellectual work and ensures that formation patterns remain reliable across different deployment scenarios.

### 3.4 Dependency Management

Components manage dependencies through explicit imports and interface contracts. The ApiService component has no dependencies on presentation or transport concerns. Interface components depend on the ApiService component through well-defined interfaces.

**Why This Matters for Formation**: Clean dependency management ensures that the core business logic that enforces user boundaries and data isolation remains independent of presentation or transport concerns. This separation prevents interface components from compromising the fundamental principles of user agency and intellectual sovereignty.

## 4. Deployment Scenarios

### 4.1 Simple Deployment

**Components**: Web + ApiService
**Use Case**: Single-user or small team deployment
**Characteristics**: Direct UI access, no external API

### 4.2 Public API Deployment

**Components**: ApiGateway + ApiService
**Use Case**: External system integration
**Characteristics**: HTTP API access, external authentication

### 4.3 AI Agent Deployment

**Components**: MCPGateway + ApiService
**Use Case**: AI agent integration and automation
**Characteristics**: MCP protocol access, AI agent authentication

### 4.4 Full Deployment

**Components**: Web + ApiGateway + MCPGateway + ApiService
**Use Case**: Complete system with UI, API, and AI agent access
**Characteristics**: Multiple access patterns, comprehensive functionality

### 4.5 Custom Compositions

The composable architecture supports any combination of interface components with the ApiService component. Additional interface components can be added following the same pattern of importing ApiService and exposing its programming interface through appropriate protocols.

## 5. Extension Points

### 5.1 Business Logic Extension

The ApiService component provides extension points for additional business logic through interface patterns. New services can be added to the ApiService component without affecting interface components.

### 5.2 Interface Extension

Interface components provide extension points for new functionality through their respective patterns. New UI components or API endpoints can be added while maintaining the same business logic interface.

### 5.3 Component Extension

New components can be added to the system by importing the ApiService component and implementing appropriate interface patterns. This enables progressive enhancement and flexible deployment scenarios.

## 6. Configuration Patterns

### 6.1 Component Configuration

Each component maintains its own configuration for component-specific concerns. The ApiService component configures business logic behavior. Interface components configure their respective behavior patterns.

### 6.2 Shared Configuration

Shared configuration is managed through the Common project and applied consistently across all components. This ensures consistent behavior while maintaining component independence.

### 6.3 Environment Configuration

Environment-specific configuration is managed through standard .NET configuration patterns. Each component can have environment-specific settings while maintaining the same interface contracts.
