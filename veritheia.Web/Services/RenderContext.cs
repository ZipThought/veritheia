using Veritheia.Common.Models;
using Veritheia.Data.Entities;

namespace veritheia.Web.Services;

/// <summary>
/// Implements the Demand-Driven Context Pattern to prevent concurrent DbContext access.
/// Components declare data needs during initialization, then a single bulk operation
/// fetches all required data before render.
/// </summary>
public class RenderContext
{
    private readonly IServiceProvider _services;
    private readonly HashSet<string> _demands = new();
    private Dictionary<string, object> _data = new();
    private bool _initialized = false;

    public RenderContext(IServiceProvider services)
    {
        _services = services;
    }

    // Demand declaration methods
    public void RequireCurrentUser() => _demands.Add("currentUser");
    public void RequireJourneys() => _demands.Add("journeys");
    public void RequirePersonas() => _demands.Add("personas");
    public void RequireStatistics() => _demands.Add("statistics");

    // Data access properties
    public UserIdentity? CurrentUser =>
        _data.ContainsKey("currentUser") ? (UserIdentity?)_data["currentUser"] : null;

    public List<Journey>? Journeys =>
        _data.ContainsKey("journeys") ? (List<Journey>?)_data["journeys"] : null;

    public List<Persona>? Personas =>
        _data.ContainsKey("personas") ? (List<Persona>?)_data["personas"] : null;

    // Statistics can be added later when JourneyStatistics type is defined
    // public JourneyStatistics? Statistics =>
    //     _data.ContainsKey("statistics") ? (JourneyStatistics?)_data["statistics"] : null;

    /// <summary>
    /// Single bulk operation that fetches all demanded data.
    /// Called once after all components have declared their needs.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        // Don't create a new scope - use the existing service provider
        // This ensures we have access to the current HttpContext

        // First, get current user if demanded
        if (_demands.Contains("currentUser"))
        {
            var authService = _services.GetRequiredService<AuthenticationService>();
            _data["currentUser"] = await authService.GetCurrentUserAsync();
        }

        // Single DB operation for all user-scoped data
        if (_data.ContainsKey("currentUser") && _data["currentUser"] != null)
        {
            var userId = ((UserIdentity)_data["currentUser"]).Id;
            
            if (_demands.Contains("journeys"))
            {
                var journeyService = _services.GetRequiredService<Veritheia.ApiService.Services.JourneyApiService>();
                _data["journeys"] = await journeyService.GetUserJourneysAsync(userId);
            }

            if (_demands.Contains("personas"))
            {
                var personaService = _services.GetRequiredService<Veritheia.ApiService.Services.PersonaApiService>();
                var personas = await personaService.GetUserPersonasAsync(userId);
                _data["personas"] = personas;
                Console.WriteLine($"[RenderContext] Fetched {personas?.Count ?? 0} personas for user {userId}");
            }

            // Statistics commented out until JourneyStatistics type is defined
            // if (_demands.Contains("statistics"))
            // {
            //     var journeyService = _services.GetRequiredService<Veritheia.ApiService.Services.JourneyApiService>();
            //     _data["statistics"] = await journeyService.GetJourneyStatisticsAsync(userId);
            // }
        }

        _initialized = true;
    }
}

/// <summary>
/// Service that manages RenderContext lifecycle within a render cycle.
/// Ensures single context per render and proper initialization.
/// </summary>
public class RenderContextService
{
    private readonly IServiceProvider _services;
    private RenderContext? _currentContext;

    public RenderContextService(IServiceProvider services)
    {
        _services = services;
    }

    public RenderContext GetOrCreateContext() =>
        _currentContext ??= new RenderContext(_services);

    public async Task InitializeContextAsync()
    {
        if (_currentContext != null)
            await _currentContext.InitializeAsync();
    }

    public void Reset() => _currentContext = null;
}