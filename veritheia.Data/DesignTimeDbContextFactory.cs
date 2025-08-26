using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veritheia.Data;

/// <summary>
/// Design-time factory for creating migrations
/// This is only used by EF Core tools when creating migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VeritheiaDbContext>
{
    public VeritheiaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VeritheiaDbContext>();

        // Use a connection string for design-time
        // This will be replaced with proper configuration in production
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=57233;Database=veritheiadb;Username=postgres;Password=B_WxmF2H}ecbVRV5Ev4G7Y",
            options => options.UseVector());

        return new VeritheiaDbContext(optionsBuilder.Options);
    }
}