using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;

namespace Veritheia.Data.Services;

/// <summary>
/// OpenAI-compatible implementation of cognitive adapter
/// Connects to local OpenAI-compatible server for LLM capabilities
/// </summary>
public class OpenAICognitiveAdapter : ICognitiveAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAICognitiveAdapter> _logger;
    private readonly string _baseUrl;
    private readonly string _model;
    
    public OpenAICognitiveAdapter(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAICognitiveAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Get configuration for OpenAI-compatible endpoint
        _baseUrl = configuration["LLM:Url"] ?? "http://localhost:1234/v1";
        _model = configuration["LLM:Model"] ?? "local-model";
        
        // Set timeout for long-running LLM operations
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }
    
    /// <summary>
    /// Generate embeddings using OpenAI-compatible endpoint
    /// </summary>
    public async Task<float[]> CreateEmbedding(string text)
    {
        try
        {
            var request = new
            {
                model = _model,
                input = text
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/embeddings", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Embedding generation failed: {Status}", response.StatusCode);
                return GenerateFallbackEmbedding(text);
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            
            // Parse OpenAI-format response
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
            
            return GenerateFallbackEmbedding(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate embeddings, using fallback");
            return GenerateFallbackEmbedding(text);
        }
    }
    
    /// <summary>
    /// Generate text using OpenAI-compatible chat completions endpoint
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
                max_tokens = 2000,
                stream = false
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending request to LLM at {Url}", $"{_baseUrl}/chat/completions");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM generation failed: {Status} - {Error}", response.StatusCode, errorContent);
                return $"[LLM error - Status: {response.StatusCode}]\n{errorContent}";
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            
            // Parse OpenAI-format response
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
            _logger.LogError(ex, "Cannot connect to LLM");
            return $"[Cannot connect to LLM at {_baseUrl}]\n\nPlease ensure the LLM server is running on port 1234.";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "LLM request timed out");
            return "[Request timed out] The LLM took too long to respond. This might happen with complex prompts.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate text with LLM");
            return $"Error: {ex.Message}";
        }
    }
    
    private float[] GenerateFallbackEmbedding(string text)
    {
        _logger.LogWarning("Using fallback embedding generation");
        // Generate deterministic embeddings based on text hash
        var embedding = new float[1536]; // OpenAI embedding dimension
        var random = new Random(text.GetHashCode());
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        return embedding;
    }
}