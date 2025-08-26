using System.Threading.Tasks;
using Veritheia.Data;
using Xunit;

namespace veritheia.Tests.TestBase;

[Collection("DatabaseTests")]
public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected VeritheiaDbContext Context = null!;

    protected DatabaseTestBase(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ResetAsync();
        Context = Fixture.CreateContext();
    }

    public virtual async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }
}