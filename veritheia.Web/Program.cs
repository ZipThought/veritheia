using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using veritheia.Web.Components;
using Veritheia.Data;
using Veritheia.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Register Database
builder.Services.AddDbContext<VeritheiaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("veritheiadb");
    options.UseNpgsql(connectionString, o => o.UseVector());
});

// Register Services
builder.Services.AddScoped<JourneyService>();
builder.Services.AddScoped<PersonaService>();
builder.Services.AddScoped<UserService>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
