using Xunit;

namespace veritheia.Tests.Integration.Web;

/// <summary>
/// Simple test to ensure Web project compiles.
/// The existence of this test with a reference to Web project
/// ensures compilation errors are caught during test runs.
/// </summary>
[Trait("Category", "Integration")]
public class WebCompilationTest
{
    [Fact]
    public void WebProject_ShouldCompile()
    {
        // This test exists solely to ensure the Web project compiles.
        // If there are compilation errors in Web project, the test suite won't build.
        var programType = typeof(veritheia.Web.Program);
        
        Assert.NotNull(programType);
        Assert.Equal("veritheia.Web", programType.Namespace);
    }
}