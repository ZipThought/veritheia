using System;
using System.Collections.Generic;

namespace Veritheia.Core.Models;

/// <summary>
/// Result pattern that tracks journey attribution and formation context
/// Every result is tied to the journey that authored it
/// </summary>
public class AuthoredResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    public Guid JourneyId { get; }
    public DateTime AuthoredAt { get; }
    public IReadOnlyList<FormationNote> FormationNotes { get; }
    
    private AuthoredResult(
        bool isSuccess, 
        T? value, 
        string error, 
        Guid journeyId,
        IEnumerable<FormationNote>? notes = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error ?? string.Empty;
        JourneyId = journeyId;
        AuthoredAt = DateTime.UtcNow;
        FormationNotes = notes?.ToList() ?? new List<FormationNote>();
    }
    
    /// <summary>
    /// Create successful result with journey attribution
    /// </summary>
    public static AuthoredResult<T> Success(
        T value, 
        Guid journeyId, 
        params FormationNote[] notes)
    {
        return new AuthoredResult<T>(true, value, string.Empty, journeyId, notes);
    }
    
    /// <summary>
    /// Create failure result with journey context
    /// </summary>
    public static AuthoredResult<T> Failure(
        string error, 
        Guid journeyId)
    {
        return new AuthoredResult<T>(false, default, error, journeyId);
    }
    
    /// <summary>
    /// Map successful value to new type while preserving journey context
    /// </summary>
    public AuthoredResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (!IsSuccess || Value == null)
            return AuthoredResult<TNew>.Failure(Error, JourneyId);
            
        return AuthoredResult<TNew>.Success(
            mapper(Value), 
            JourneyId, 
            FormationNotes.ToArray());
    }
}

/// <summary>
/// Note about how understanding was formed
/// </summary>
public record FormationNote(
    string Type,      // "Insight", "Discovery", "Synthesis"
    string Content,   // The actual note
    DateTime NotedAt = default)
{
    public DateTime NotedAt { get; init; } = 
        NotedAt == default ? DateTime.UtcNow : NotedAt;
}