#if TEST_BUILD
using Veritheia.Core.Interfaces;

namespace Veritheia.Tests.Helpers;

/// <summary>
/// Test-only cognitive adapter interface for integration tests when LLM is unavailable.
/// CRITICAL: This interface must NEVER be accessible in production builds.
/// Only use for testing data flow paths, NOT for formation validity testing.
/// </summary>
public interface ITestCognitiveAdapter : ICognitiveAdapter
{
    /// <summary>
    /// Enables deterministic test data generation for repeatable test outcomes.
    /// All generated data is clearly marked as fake for test purposes only.
    /// </summary>
    void SetDeterministicMode(bool enabled);
    
    /// <summary>
    /// Sets a seed for deterministic fake data generation in tests.
    /// This enables repeatable test scenarios without neural processing.
    /// </summary>
    void SetTestSeed(int seed);
    
    /// <summary>
    /// Configures whether to simulate failures for error handling tests.
    /// Enables testing of exception paths without actual service failures.
    /// </summary>
    void SimulateServiceFailure(bool shouldFail);
}
#endif