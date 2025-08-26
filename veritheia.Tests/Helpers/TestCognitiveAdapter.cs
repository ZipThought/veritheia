#if TEST_BUILD
using Veritheia.Core.Interfaces;

namespace Veritheia.Tests.Helpers;

/// <summary>
/// Test-only implementation of cognitive adapter for integration tests.
/// 
/// CRITICAL WARNINGS:
/// - This class must NEVER be accessible in production builds
/// - Only use when LLM services are unavailable (CI environments, resource-constrained dev machines)
/// - Generated data is FAKE and must not be used for formation validity testing
/// - Only validates data flow paths, not formation quality
/// - All outputs are clearly marked as test-generated
/// 
/// This implementation provides deterministic fake data for testing integration paths
/// when real neural processing is not available, while maintaining clear boundaries
/// between test infrastructure and production formation processes.
/// </summary>
public class TestCognitiveAdapter : ITestCognitiveAdapter
{
    private bool _deterministicMode = true;
    private int _testSeed = 42;
    private Random _random;
    private bool _shouldSimulateFailure = false;

    public TestCognitiveAdapter()
    {
        _random = new Random(_testSeed);
    }

    /// <summary>
    /// Generates deterministic fake embeddings for testing data flow paths only.
    /// 
    /// WARNING: These are NOT real neural embeddings and must never be used
    /// for actual formation or semantic understanding. They are purely for
    /// testing that data flows correctly through the system.
    /// </summary>
    public Task<float[]> CreateEmbedding(string text)
    {
        if (_shouldSimulateFailure)
        {
            throw new InvalidOperationException("[TEST ADAPTER] Simulated embedding generation failure");
        }

        // Generate deterministic fake embedding based on text content
        // This enables repeatable test outcomes while being clearly fake
        var embedding = new float[1536]; // OpenAI embedding dimension

        if (_deterministicMode)
        {
            // Use text hash for deterministic output in tests
            var textHash = text.GetHashCode();
            var deterministicRandom = new Random(textHash + _testSeed);

            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(deterministicRandom.NextDouble() * 2 - 1);
            }
        }
        else
        {
            // Use instance random for variation
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(_random.NextDouble() * 2 - 1);
            }
        }

        return Task.FromResult(embedding);
    }

    /// <summary>
    /// Generates deterministic fake text responses for testing data flow paths only.
    /// 
    /// WARNING: These are NOT real neural responses and must never be used
    /// for actual formation or assessment. They are purely for testing that
    /// data flows correctly through the system.
    /// </summary>
    public Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null)
    {
        if (_shouldSimulateFailure)
        {
            throw new InvalidOperationException("[TEST ADAPTER] Simulated text generation failure");
        }

        // Generate deterministic fake response based on prompt content
        // Clearly marked as test-generated to prevent confusion
        var promptHash = prompt.GetHashCode();
        if (systemPrompt != null)
        {
            promptHash ^= systemPrompt.GetHashCode();
        }

        // Create deterministic but varied responses for different prompt types
        if (prompt.Contains("relevance", StringComparison.OrdinalIgnoreCase))
        {
            var relevanceScore = Math.Abs(promptHash % 100) / 100.0f;
            return Task.FromResult($"[TEST GENERATED] Relevance Assessment\nScore: {relevanceScore:F2}\nReasoning: This is a deterministic test response for relevance assessment. Not real neural evaluation.");
        }

        if (prompt.Contains("contribution", StringComparison.OrdinalIgnoreCase))
        {
            var contributionScore = Math.Abs((promptHash + 1) % 100) / 100.0f;
            return Task.FromResult($"[TEST GENERATED] Contribution Assessment\nScore: {contributionScore:F2}\nReasoning: This is a deterministic test response for contribution assessment. Not real neural evaluation.");
        }

        if (prompt.Contains("extract", StringComparison.OrdinalIgnoreCase) || prompt.Contains("semantic", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult($"[TEST GENERATED] Semantic Extraction\nTopics: [test-topic-1, test-topic-2]\nEntities: [test-entity-1, test-entity-2]\nKeywords: [test-keyword-1, test-keyword-2]\nNote: This is deterministic test data, not real semantic understanding.");
        }

        // Default response for any other prompts
        return Task.FromResult($"[TEST GENERATED] Response to prompt hash {Math.Abs(promptHash)}: This is a deterministic test response. Not real neural processing.");
    }

    public void SetDeterministicMode(bool enabled)
    {
        _deterministicMode = enabled;
    }

    public void SetTestSeed(int seed)
    {
        _testSeed = seed;
        _random = new Random(seed);
    }

    public void SimulateServiceFailure(bool shouldFail)
    {
        _shouldSimulateFailure = shouldFail;
    }
}
#endif