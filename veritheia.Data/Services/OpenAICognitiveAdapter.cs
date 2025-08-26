using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.Exceptions;

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
                var statusCode = response.StatusCode;
                var reasonPhrase = response.ReasonPhrase ?? "Unknown error";
                _logger.LogError("Embedding generation failed: {Status} - {Reason}", statusCode, reasonPhrase);
                
                throw new EmbeddingGenerationException(
                    text,
                    $"HTTP {(int)statusCode} {reasonPhrase}. LLM service returned error status.",
                    innerException: new HttpRequestException($"HTTP {(int)statusCode} {reasonPhrase}")
                );
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
            
            throw new EmbeddingGenerationException(
                text,
                "Invalid response format from LLM service. Expected OpenAI-compatible embedding response format.",
                innerException: new InvalidDataException("Missing 'data' array or 'embedding' property in response")
            );
        }
        catch (EmbeddingGenerationException)
        {
            // Re-throw our specific exceptions to maintain formation context
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during embedding generation");
            
            throw new EmbeddingGenerationException(
                text,
                $"Unexpected error: {ex.Message}",
                innerException: ex
            );
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
                var statusCode = response.StatusCode;
                var reasonPhrase = response.ReasonPhrase ?? "Unknown error";
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM generation failed: {Status} - {Reason} - {Error}", statusCode, reasonPhrase, errorContent);
                
                throw new TextGenerationException(
                    prompt,
                    $"HTTP {(int)statusCode} {reasonPhrase}. LLM service error: {errorContent}",
                    innerException: new HttpRequestException($"HTTP {(int)statusCode} {reasonPhrase}: {errorContent}")
                );
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
                        return contentElement.GetString() ?? 
                               throw new TextGenerationException(prompt, "LLM returned null content in response");
                    }
                }
            }
            
            throw new TextGenerationException(
                prompt,
                "Invalid response format from LLM service. Expected OpenAI-compatible chat completion response format.",
                innerException: new InvalidDataException("Missing 'choices' array, 'message' object, or 'content' property in response")
            );
        }
        catch (TextGenerationException)
        {
            // Re-throw our specific exceptions to maintain formation context
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot connect to LLM at {BaseUrl}", _baseUrl);
            
            throw new TextGenerationException(
                prompt,
                $"Cannot connect to LLM service at {_baseUrl}. Service may be down or unreachable.",
                innerException: ex
            );
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "LLM request timed out");
            
            throw new TextGenerationException(
                prompt,
                "LLM request timed out. The service took too long to respond, which may happen with complex prompts.",
                innerException: ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during text generation");
            
            throw new TextGenerationException(
                prompt,
                $"Unexpected error during text generation: {ex.Message}",
                innerException: ex
            );
        }
    }
}