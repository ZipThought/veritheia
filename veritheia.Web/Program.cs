using veritheia.Web.Components;
using veritheia.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Veritheia.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddAuthorization();

// Add HTTP context accessor for authentication
builder.Services.AddHttpContextAccessor();

// Register ApiService business logic components directly
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register Web-specific services
builder.Services.AddScoped<JourneyApiService>();
builder.Services.AddScoped<PersonaApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<ProcessConfigurationService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();