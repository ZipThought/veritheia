using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Platform Service: Embedding Generation
/// MVP 2.2.3: Embedding Generation
/// </summary>
public class EmbeddingService
{
    private readonly VeritheiaDbContext _db;
    private readonly ICognitiveAdapter? _cognitive;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        VeritheiaDbContext dbContext,
        ILogger<EmbeddingService> logger,
        ICognitiveAdapter? cognitive = null)
    {
        _db = dbContext;
        _logger = logger;
        _cognitive = cognitive;
    }

    /// <summary>
    /// Generate embedding for a journey document segment
    /// </summary>
    public async Task<bool> GenerateEmbeddingForSegmentAsync(Guid segmentId)
    {
        if (_cognitive == null)
        {
            _logger.LogWarning("No cognitive adapter configured, skipping embedding generation");
            return false;
        }

        var segment = await _db.JourneyDocumentSegments
            .Include(s => s.Journey)
            .FirstOrDefaultAsync(s => s.Id == segmentId);

        if (segment == null)
        {
            _logger.LogError("Segment {SegmentId} not found", segmentId);
            return false;
        }

        try
        {
            // Generate embedding with journey context
            var contextualText = BuildContextualText(segment);
            var embedding = await _cognitive.CreateEmbedding(contextualText);

            // Determine vector dimension
            var dimension = embedding.Length;

            // Create search index entry
            var searchIndex = new SearchIndex
            {
                Id = Guid.CreateVersion7(),
                SegmentId = segmentId,
                VectorModel = _cognitive.GetType().Name
            };

            _db.SearchIndexes.Add(searchIndex);

            // Store in appropriate vector table based on dimension
            switch (dimension)
            {
                case 1536:
                    var vector1536 = new SearchVector1536
                    {
                        IndexId = searchIndex.Id,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                    _db.SearchVectors1536.Add(vector1536);
                    break;

                case 768:
                    var vector768 = new SearchVector768
                    {
                        IndexId = searchIndex.Id,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                    _db.SearchVectors768.Add(vector768);
                    break;

                case 384:
                    var vector384 = new SearchVector384
                    {
                        IndexId = searchIndex.Id,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                    _db.SearchVectors384.Add(vector384);
                    break;

                default:
                    _logger.LogError("Unsupported embedding dimension: {Dimension}", dimension);
                    return false;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Generated {Dimension}-dim embedding for segment {SegmentId}",
                dimension, segmentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for segment {SegmentId}", segmentId);
            return false;
        }
    }

    /// <summary>
    /// Build contextual text including journey framework
    /// </summary>
    private string BuildContextualText(JourneyDocumentSegment segment)
    {
        // Include journey context for better embeddings
        var context = $"Journey Purpose: {segment.Journey.Purpose}\n";
        context += $"Segment Type: {segment.SegmentType}\n";
        context += $"Content: {segment.SegmentContent}";

        return context;
    }

    /// <summary>
    /// Find similar segments using vector search
    /// </summary>
    public async Task<SimilarSegment[]> FindSimilarSegmentsAsync(
        Guid journeyId,
        float[] queryEmbedding,
        int limit = 10)
    {
        // This would use pgvector's <=> operator for similarity
        // For now, returning journey segments as example
        var segments = await _db.JourneyDocumentSegments
            .Where(s => s.JourneyId == journeyId)
            .Take(limit)
            .Select(s => new SimilarSegment
            {
                SegmentId = s.Id,
                DocumentId = s.DocumentId,
                Content = s.SegmentContent,
                SimilarityScore = 0.95 // Would be calculated by pgvector
            })
            .ToArrayAsync();

        return segments;
    }
}

public class SimilarSegment
{
    public Guid SegmentId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
}