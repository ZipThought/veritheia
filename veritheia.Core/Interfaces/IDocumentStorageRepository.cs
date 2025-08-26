using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Veritheia.Core.Interfaces;

/// <summary>
/// Domain-shaped contract for document file storage operations
/// Abstracts filesystem/S3/cloud storage - anything NOT in PostgreSQL
/// This is a true repository pattern because it abstracts external resources
/// </summary>
public interface IDocumentStorageRepository
{
    /// <summary>
    /// Store a document file and return its storage path
    /// </summary>
    Task<string> StoreDocumentAsync(
        Stream content,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve document content by storage path
    /// </summary>
    Task<Stream> GetDocumentContentAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if document exists at path
    /// </summary>
    Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete document from storage
    /// </summary>
    Task DeleteDocumentAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document metadata without retrieving content
    /// </summary>
    Task<DocumentStorageMetadata> GetMetadataAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata about stored document
/// </summary>
public record DocumentStorageMetadata(
    long SizeInBytes,
    DateTime CreatedUtc,
    DateTime LastModifiedUtc,
    string ContentHash);