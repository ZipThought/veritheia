using veritheia.Web.Components;
using veritheia.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Veritheia.Data;
using Veritheia.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Remove LoginPath to prevent redirect loops
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddAuthorization();

// Register Data layer services (encapsulated)
builder.Services.AddDataServices(builder.Configuration, builder.Environment);

// Add HTTP context accessor for authentication
builder.Services.AddHttpContextAccessor();


// Register ApiService Services
builder.Services.AddScoped<Veritheia.ApiService.Services.UserApiService>();
builder.Services.AddScoped<Veritheia.ApiService.Services.JourneyApiService>();
builder.Services.AddScoped<Veritheia.ApiService.Services.PersonaApiService>();

// Web Services
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<ProcessConfigurationService>();
builder.Services.AddScoped<RenderContextService>();

// Add services to the container.
builder.Services.AddControllers(); // Add controller support for authentication
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure detailed errors for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options => options.DetailedErrors = true);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Global exception handler to log all errors
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");
        throw; // Re-throw to let ASP.NET Core handle it
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapDefaultEndpoints();

app.Run();