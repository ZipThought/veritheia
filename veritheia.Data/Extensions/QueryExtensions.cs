using Microsoft.EntityFrameworkCore;
using Veritheia.Data.Entities;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Extensions;

/// <summary>
/// Query extensions that enforce partition boundaries and provide journey-scoped operations
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Enforces user partition boundary - all queries must scope to a specific user
    /// </summary>
    public static IQueryable<T> ForUser<T>(
        this IQueryable<T> query,
        Guid userId) where T : class, IUserOwned
    {
        return query.Where(e => e.UserId == userId);
    }

    /// <summary>
    /// Scopes journey document segments to a specific journey within user partition
    /// </summary>
    public static IQueryable<JourneyDocumentSegment> ForJourney(
        this IQueryable<JourneyDocumentSegment> segments,
        Guid userId,
        Guid journeyId)
    {
        return segments
            .Where(s => s.UserId == userId)
            .Where(s => s.JourneyId == journeyId);
    }

    /// <summary>
    /// Includes assessments for journey document segments
    /// </summary>
    public static IQueryable<JourneyDocumentSegment> WithAssessments(
        this IQueryable<JourneyDocumentSegment> segments)
    {
        return segments.Include(s => s.Assessments);
    }

    /// <summary>
    /// Includes search indexes for journey document segments
    /// </summary>
    public static IQueryable<JourneyDocumentSegment> WithSearchIndexes(
        this IQueryable<JourneyDocumentSegment> segments)
    {
        return segments.Include(s => s.SearchIndexes);
    }

    /// <summary>
    /// Scopes journals to a specific journey within user partition
    /// </summary>
    public static IQueryable<Journal> ForJourney(
        this IQueryable<Journal> journals,
        Guid userId,
        Guid journeyId)
    {
        return journals
            .Where(j => j.UserId == userId)
            .Where(j => j.JourneyId == journeyId);
    }

    /// <summary>
    /// Includes journal entries for journals
    /// </summary>
    public static IQueryable<Journal> WithEntries(
        this IQueryable<Journal> journals)
    {
        return journals.Include(j => j.Entries);
    }

    /// <summary>
    /// Scopes formations to a specific journey within user partition
    /// </summary>
    public static IQueryable<JourneyFormation> ForJourney(
        this IQueryable<JourneyFormation> formations,
        Guid userId,
        Guid journeyId)
    {
        return formations
            .Where(f => f.UserId == userId)
            .Where(f => f.JourneyId == journeyId);
    }

    /// <summary>
    /// Scopes process executions to a specific journey within user partition
    /// </summary>
    public static IQueryable<ProcessExecution> ForJourney(
        this IQueryable<ProcessExecution> executions,
        Guid userId,
        Guid journeyId)
    {
        return executions
            .Where(e => e.UserId == userId)
            .Where(e => e.JourneyId == journeyId);
    }

    /// <summary>
    /// Includes process result for process executions
    /// </summary>
    public static IQueryable<ProcessExecution> WithResult(
        this IQueryable<ProcessExecution> executions)
    {
        return executions.Include(e => e.Result);
    }

    /// <summary>
    /// Scopes documents to a specific scope within user partition
    /// </summary>
    public static IQueryable<Document> ForScope(
        this IQueryable<Document> documents,
        Guid userId,
        Guid? scopeId)
    {
        var query = documents.Where(d => d.UserId == userId);

        if (scopeId.HasValue)
        {
            query = query.Where(d => d.ScopeId == scopeId.Value);
        }

        return query;
    }

    /// <summary>
    /// Includes metadata for documents
    /// </summary>
    public static IQueryable<Document> WithMetadata(
        this IQueryable<Document> documents)
    {
        return documents.Include(d => d.Metadata);
    }

    /// <summary>
    /// Includes journey segments for documents
    /// </summary>
    public static IQueryable<Document> WithJourneySegments(
        this IQueryable<Document> documents)
    {
        return documents.Include(d => d.JourneySegments);
    }

    /// <summary>
    /// Orders entities by creation time (newest first)
    /// </summary>
    public static IQueryable<T> OrderByCreatedAtDesc<T>(
        this IQueryable<T> query) where T : BaseEntity
    {
        return query.OrderByDescending(e => e.CreatedAt);
    }

    /// <summary>
    /// Orders entities by creation time (oldest first)
    /// </summary>
    public static IQueryable<T> OrderByCreatedAtAsc<T>(
        this IQueryable<T> query) where T : BaseEntity
    {
        return query.OrderBy(e => e.CreatedAt);
    }
}
