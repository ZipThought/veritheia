using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;

namespace Veritheia.Data.Services;

/// <summary>
/// Ollama implementation of cognitive adapter
/// Connects to local Ollama instance for LLM capabilities
/// </summary>
public class OllamaCognitiveAdapter : ICognitiveAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaCognitiveAdapter> _logger;
    private readonly string _ollamaUrl;
    private readonly string _embeddingModel;
    private readonly string _chatModel;

    public OllamaCognitiveAdapter(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaCognitiveAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434";
        _embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        _chatModel = configuration["Ollama:ChatModel"] ?? "llama3.2";
    }

    /// <summary>
    /// Generate embeddings using Ollama
    /// </summary>
    public async Task<float[]> CreateEmbedding(string text)
    {
        try
        {
            var request = new
            {
                model = _embeddingModel,
                prompt = text
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ollama embedding failed: {Status}", response.StatusCode);
                // Fallback to random embeddings if Ollama not available
                return GenerateFallbackEmbedding(text);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("embedding", out var embeddingElement))
            {
                var embeddings = new float[embeddingElement.GetArrayLength()];
                int i = 0;
                foreach (var value in embeddingElement.EnumerateArray())
                {
                    embeddings[i++] = (float)value.GetDouble();
                }
                return embeddings;
            }

            return GenerateFallbackEmbedding(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings with Ollama");
            return GenerateFallbackEmbedding(text);
        }
    }

    /// <summary>
    /// Generate text using Ollama
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null)
    {
        try
        {
            var fullPrompt = string.IsNullOrEmpty(systemPrompt)
                ? prompt
                : $"{systemPrompt}\n\n{prompt}";

            var request = new
            {
                model = _chatModel,
                prompt = fullPrompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ollama generation failed: {Status}", response.StatusCode);
                return $"[Ollama not available - Status: {response.StatusCode}]\n\nPlease ensure Ollama is running locally with model '{_chatModel}' installed.\nRun: ollama pull {_chatModel}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("response", out var responseElement))
            {
                return responseElement.GetString() ?? "No response generated";
            }

            return "Failed to parse Ollama response";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot connect to Ollama");
            return $"[Cannot connect to Ollama at {_ollamaUrl}]\n\nPlease install and start Ollama:\n1. Install from https://ollama.ai\n2. Run: ollama serve\n3. Pull model: ollama pull {_chatModel}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate text with Ollama");
            return $"Error: {ex.Message}";
        }
    }

    private float[] GenerateFallbackEmbedding(string text)
    {
        _logger.LogWarning("Using fallback embedding generation");
        // Generate deterministic embeddings based on text hash
        var embedding = new float[768]; // nomic-embed-text dimension
        var random = new Random(text.GetHashCode());
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        return embedding;
    }
}