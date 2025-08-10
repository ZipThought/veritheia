using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;

namespace Veritheia.Data.Services;

/// <summary>
/// SKELETON: Mock implementation of cognitive adapter for development
/// REALITY: Returns fake data, no actual AI integration
/// TODO: Replace with real implementation (OpenAI/Ollama/SemanticKernel)
/// </summary>
public class MockCognitiveAdapter : ICognitiveAdapter
{
    private readonly ILogger<MockCognitiveAdapter> _logger;
    
    public MockCognitiveAdapter(ILogger<MockCognitiveAdapter> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// SKELETON: Returns fake embedding vector
    /// </summary>
    public async Task<float[]> CreateEmbedding(string text)
    {
        _logger.LogWarning("MOCK: Returning fake embedding for text of length {Length}", text.Length);
        
        // Return fake 1536-dimensional vector (OpenAI ada-002 size)
        var fakeEmbedding = new float[1536];
        var random = new Random(text.GetHashCode());
        for (int i = 0; i < fakeEmbedding.Length; i++)
        {
            fakeEmbedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        
        await Task.Delay(100); // Simulate API latency
        return fakeEmbedding;
    }
    
    /// <summary>
    /// SKELETON: Returns canned response
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null)
    {
        _logger.LogWarning("MOCK: Returning canned response for prompt of length {Length}", prompt.Length);
        
        await Task.Delay(200); // Simulate API latency
        
        return $@"[MOCK RESPONSE - Real LLM integration needed]

This is a placeholder response from MockCognitiveAdapter.
In a real implementation, this would:
1. Send the prompt to an LLM (OpenAI/Anthropic/Ollama)
2. Apply the system prompt for context
3. Return actual generated text

Your prompt was: {prompt.Substring(0, Math.Min(100, prompt.Length))}...
System prompt: {systemPrompt ?? "None"}

To implement real cognitive capabilities:
- Install OpenAI/Anthropic SDK
- Or use Semantic Kernel
- Or integrate with local Ollama
- Configure API keys in appsettings";
    }
}