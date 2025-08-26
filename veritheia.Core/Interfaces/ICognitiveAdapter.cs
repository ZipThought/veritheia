using System.Threading.Tasks;

namespace Veritheia.Core.Interfaces;

/// <summary>
/// Cognitive adapter interface for LLM integration
/// MVP 5.1.1: Adapter Interface
/// </summary>
public interface ICognitiveAdapter
{
    /// <summary>
    /// Generate embeddings for text
    /// </summary>
    Task<float[]> CreateEmbedding(string text);
    
    /// <summary>
    /// Generate text completion
    /// </summary>
    Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null);
}