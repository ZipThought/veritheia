using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Document management service
/// Coordinates between database (metadata) and file storage (content)
/// </summary>
public class DocumentService
{
    private readonly VeritheiaDbContext _db;
    private readonly IDocumentStorageRepository _storage;

    public DocumentService(
        VeritheiaDbContext dbContext,
        IDocumentStorageRepository storage)
    {
        _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <summary>
    /// Upload a document for a user
    /// </summary>
    public async Task<Document> UploadDocumentAsync(
        Stream content,
        string fileName,
        string mimeType,
        Guid userId,
        Guid? scopeId = null)
    {
        // Store file content
        var storagePath = await _storage.StoreDocumentAsync(content, fileName, mimeType);

        // Get file metadata
        var metadata = await _storage.GetMetadataAsync(storagePath);

        // Create database record
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FileName = fileName,
            MimeType = mimeType,
            FilePath = storagePath,
            FileSize = metadata.SizeInBytes,
            UploadedAt = DateTime.UtcNow,
            ScopeId = scopeId
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return document;
    }

    /// <summary>
    /// Get documents for a user
    /// User isolation enforced through query
    /// </summary>
    public async Task<List<Document>> GetUserDocumentsAsync(Guid userId)
    {
        return await _db.Documents
            .Where(d => d.UserId == userId)
            .Include(d => d.Scope)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get document content
    /// </summary>
    public async Task<Stream> GetDocumentContentAsync(Guid documentId, Guid userId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
            throw new UnauthorizedAccessException($"Document {documentId} not found or access denied");

        return await _storage.GetDocumentContentAsync(document.FilePath);
    }

    /// <summary>
    /// Delete document (metadata and content)
    /// </summary>
    public async Task DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
            throw new UnauthorizedAccessException($"Document {documentId} not found or access denied");

        // Delete file content
        await _storage.DeleteDocumentAsync(document.FilePath);

        // Delete database record (cascade will handle related records)
        _db.Documents.Remove(document);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Check if document has projections in a journey
    /// </summary>
    public async Task<bool> HasJourneyProjectionsAsync(Guid documentId, Guid journeyId)
    {
        return await _db.JourneyDocumentSegments
            .AnyAsync(s => s.DocumentId == documentId && s.JourneyId == journeyId);
    }
}