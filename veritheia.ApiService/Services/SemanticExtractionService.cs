using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;

namespace Veritheia.ApiService.Services;

/// <summary>
/// Service for extracting semantic information from abstracts using LLM
/// </summary>
public class SemanticExtractionService
{
    private readonly ICognitiveAdapter _cognitiveAdapter;
    private readonly ILogger<SemanticExtractionService> _logger;

    public SemanticExtractionService(
        ICognitiveAdapter cognitiveAdapter,
        ILogger<SemanticExtractionService> logger)
    {
        _cognitiveAdapter = cognitiveAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Extract topics, entities, and keywords from abstract
    /// </summary>
    public async Task<SemanticExtraction> ExtractSemanticsAsync(string abstractText)
    {
        if (string.IsNullOrWhiteSpace(abstractText))
        {
            return new SemanticExtraction();
        }

        var prompt = CreateExtractionPrompt(abstractText);
        var response = await _cognitiveAdapter.GenerateTextAsync(prompt);

        return ParseExtractionResponse(response);
    }

    private string CreateExtractionPrompt(string abstractText)
    {
        return $@"Extract semantic information from this research abstract. Provide your response as valid JSON only.

Abstract: {abstractText}

Extract:
1. Topics: 3-5 main research topics or themes
2. Entities: Key technical terms, methods, systems, or concepts mentioned
3. Keywords: Important domain-specific terms

Respond with JSON in this exact format:
{{
  ""topics"": [""topic1"", ""topic2"", ""topic3""],
  ""entities"": [""entity1"", ""entity2"", ""entity3""],
  ""keywords"": [""keyword1"", ""keyword2"", ""keyword3""]
}}

JSON Response:";
    }

    private SemanticExtraction ParseExtractionResponse(string response)
    {
        // Try to find JSON in the response
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var jsonText = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var parsed = JsonSerializer.Deserialize<SemanticExtractionJson>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize JSON: {jsonText}");

            return new SemanticExtraction
            {
                Topics = parsed.Topics?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList() ?? new List<string>(),
                Entities = parsed.Entities?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                Keywords = parsed.Keywords?.Where(k => !string.IsNullOrWhiteSpace(k)).ToList() ?? new List<string>()
            };
        }

        // If no JSON found, throw exception instead of fallback
        throw new InvalidOperationException($"No valid JSON found in LLM response: {response}");
    }

}

/// <summary>
/// Extracted semantic information
/// </summary>
public class SemanticExtraction
{
    public List<string> Topics { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
}

/// <summary>
/// JSON structure for parsing LLM response
/// </summary>
internal class SemanticExtractionJson
{
    public List<string>? Topics { get; set; }
    public List<string>? Entities { get; set; }
    public List<string>? Keywords { get; set; }
}