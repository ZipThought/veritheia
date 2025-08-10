using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Veritheia.Core.Interfaces;

/// <summary>
/// Domain-shaped contract for journey formation tracking
/// This provides SEMANTIC awareness (why/how understanding formed)
/// Not mechanical scoping (which is handled by EF Core directly)
/// </summary>
public interface IJourneyFormationRepository
{
    /// <summary>
    /// Record that a document was encountered during journey formation
    /// Tracks WHY the document was accessed (semantic awareness)
    /// </summary>
    Task RecordDocumentEncounterAsync(
        Guid journeyId,
        Guid documentId,
        string encounterContext,  // WHY was this accessed?
        string formationPurpose,   // WHAT understanding is being formed?
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get documents that contributed to understanding formation
    /// Returns documents with their formation context
    /// </summary>
    Task<IReadOnlyList<FormativeDocument>> GetFormativeDocumentsAsync(
        Guid journeyId,
        DateTime since,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Record a formation marker - a moment of understanding
    /// </summary>
    Task RecordFormationMarkerAsync(
        Guid journeyId,
        string insight,
        string formationType,
        IEnumerable<Guid> contributingDocumentIds,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Find similar formations using semantic search
    /// Uses pgvector for similarity but wrapped in domain language
    /// </summary>
    Task<IReadOnlyList<SimilarFormation>> FindSimilarFormationsAsync(
        Guid journeyId,
        float[] queryEmbedding,
        double semanticThreshold = 0.7,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Document with its formation context
/// </summary>
public record FormativeDocument(
    Guid DocumentId,
    string FileName,
    string EncounterContext,
    string FormationPurpose,
    DateTime EncounteredAt,
    double RelevanceScore);

/// <summary>
/// Similar formation found through semantic search
/// </summary>
public record SimilarFormation(
    Guid FormationId,
    Guid JourneyId,
    string Insight,
    double SimilarityScore,
    DateTime FormedAt);