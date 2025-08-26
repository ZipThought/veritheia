using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;

namespace Veritheia.Data.Services;

/// <summary>
/// Local LLM implementation using OpenAI-compatible API
/// Connects to local LLM server (LM Studio, Ollama, etc.)
/// </summary>
public class LocalLLMAdapter : ICognitiveAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalLLMAdapter> _logger;
    private readonly string _llmUrl;
    private readonly string _model;

    public LocalLLMAdapter(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LocalLLMAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _llmUrl = configuration["LocalLLM:Url"] ?? "http://localhost:1234";
        _model = configuration["LocalLLM:Model"] ?? "gemma-3-12b-it";
    }

    /// <summary>
    /// Generate embeddings - fallback to deterministic for now
    /// Most local LLMs don't have embedding endpoints
    /// </summary>
    public async Task<float[]> CreateEmbedding(string text)
    {
        try
        {
            // Try OpenAI-compatible embeddings endpoint
            var request = new
            {
                model = _model,
                input = text
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_llmUrl}/v1/embeddings", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("data", out var dataElement))
                {
                    var firstEmbedding = dataElement.EnumerateArray().FirstOrDefault();
                    if (firstEmbedding.TryGetProperty("embedding", out var embeddingElement))
                    {
                        var embeddings = new float[embeddingElement.GetArrayLength()];
                        int i = 0;
                        foreach (var value in embeddingElement.EnumerateArray())
                        {
                            embeddings[i++] = (float)value.GetDouble();
                        }
                        return embeddings;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Embeddings endpoint not available, using fallback");
        }

        // Fallback to deterministic embeddings
        return GenerateFallbackEmbedding(text);
    }

    /// <summary>
    /// Generate text using OpenAI-compatible chat completions API
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null)
    {
        try
        {
            var messages = new List<object>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new { role = "system", content = systemPrompt });
            }

            messages.Add(new { role = "user", content = prompt });

            var request = new
            {
                model = _model,
                messages = messages,
                temperature = 0.7,
                max_tokens = -1,
                stream = false
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_llmUrl}/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("LLM generation failed: {Status}", response.StatusCode);
                return $"[LLM Error - Status: {response.StatusCode}]\n\nPlease check that the LLM server is running at {_llmUrl}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("choices", out var choicesElement))
            {
                var firstChoice = choicesElement.EnumerateArray().FirstOrDefault();
                if (firstChoice.TryGetProperty("message", out var messageElement))
                {
                    if (messageElement.TryGetProperty("content", out var contentElement))
                    {
                        return contentElement.GetString() ?? "No response generated";
                    }
                }
            }

            return "Failed to parse LLM response";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot connect to LLM server");
            return $"[Cannot connect to LLM at {_llmUrl}]\n\nError: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate text with LLM");
            return $"Error: {ex.Message}";
        }
    }

    private float[] GenerateFallbackEmbedding(string text)
    {
        _logger.LogDebug("Using fallback embedding generation");
        // Generate deterministic embeddings based on text hash
        var embedding = new float[768]; // Standard embedding size
        var random = new Random(text.GetHashCode());
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        return embedding;
    }
}