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
    /// Upload full text for existing document or create new document with PDF
    /// </summary>
    public async Task<Document> UploadFullTextAsync(
        Stream content,
        string fileName,
        string mimeType,
        Guid userId,
        Guid? existingDocumentId = null,
        string? title = null)
    {
        Document document;

        if (existingDocumentId.HasValue)
        {
            // Attach full text to existing document
            document = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == existingDocumentId.Value && d.UserId == userId)
                ?? throw new UnauthorizedAccessException("Document not found or access denied");
        }
        else
        {
            // Create new document from PDF
            document = new Document
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                FileName = fileName,
                MimeType = mimeType,
                FilePath = string.Empty, // Will be set after storage
                FileSize = 0, // Will be set after storage
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _db.Documents.Add(document);
        }

        // Store file content
        var storagePath = await _storage.StoreDocumentAsync(content, fileName, mimeType);
        var storageMetadata = await _storage.GetMetadataAsync(storagePath);

        // Update document with file storage info
        document.FilePath = storagePath;
        document.FileSize = storageMetadata.SizeInBytes;

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
    /// Get document full text content (if available)
    /// </summary>
    public async Task<Stream?> GetDocumentFullTextAsync(Guid documentId, Guid userId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
            throw new UnauthorizedAccessException($"Document {documentId} not found or access denied");

        if (string.IsNullOrEmpty(document.FilePath))
            return null;  // No full text available

        return await _storage.GetDocumentContentAsync(document.FilePath);
    }

    /// <summary>
    /// Add document to corpus with deduplication by DOI
    /// </summary>
    public async Task<Document> AddDocumentToCorpusAsync(
        Guid userId,
        string title,
        string? abstractText,
        string? authors,
        string? doi,
        int? year,
        string? venue,
        string? keywords)
    {
        // Check for duplicate by DOI if provided (check in metadata)
        if (!string.IsNullOrWhiteSpace(doi))
        {
            var existing = await _db.DocumentMetadata
                .Include(dm => dm.Document)
                .Where(dm => dm.UserId == userId)
                .Where(dm => dm.DOI == doi)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Document already exists, return existing
                return existing.Document;
            }
        }

        // Create Document entity (file placeholder - no actual file yet from CSV)
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FileName = $"{title?.Replace("/", "-").Replace(":", "-") ?? "untitled"}.pdf", // Placeholder filename
            MimeType = "application/pdf", // Expected future upload type
            FilePath = string.Empty, // No file yet
            FileSize = 0, // No file yet
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Create DocumentMetadata entity with bibliographic data from CSV
        var metadata = new DocumentMetadata
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            DocumentId = document.Id,
            Title = title,
            Abstract = abstractText,
            Authors = authors?.Split(',').Select(a => a.Trim()).ToArray(),
            DOI = doi,
            PublicationDate = year.HasValue ? new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc) : null,
            ExtendedMetadata = new Dictionary<string, object>
            {
                ["venue"] = venue ?? string.Empty,
                ["keywords"] = keywords ?? string.Empty,
                ["source"] = "csv_import"
            }
        };

        _db.Documents.Add(document);
        _db.DocumentMetadata.Add(metadata);
        await _db.SaveChangesAsync();

        return document;
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

        // Delete file content if exists
        if (!string.IsNullOrEmpty(document.FilePath))
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