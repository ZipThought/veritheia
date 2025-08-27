using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.ApiService.Services;

/// <summary>
/// Service for managing document metadata in user corpus
/// </summary>
public class DocumentMetadataService
{
    private readonly VeritheiaDbContext _db;
    private readonly ILogger<DocumentMetadataService> _logger;

    public DocumentMetadataService(
        VeritheiaDbContext dbContext,
        ILogger<DocumentMetadataService> logger)
    {
        _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create or update document metadata
    /// </summary>
    public async Task<DocumentMetadata> CreateOrUpdateMetadataAsync(
        Guid userId,
        Guid documentId,
        string title,
        string[] authors,
        string? abstractText,
        string[]? keywords,
        DateTime? publicationDate = null,
        string? publisher = null,
        string? doi = null)
    {
        // Check if document exists and belongs to user
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
        {
            throw new UnauthorizedAccessException($"Document {documentId} not found or access denied");
        }

        // Check if metadata already exists
        var metadata = await _db.DocumentMetadata
            .FirstOrDefaultAsync(m => m.DocumentId == documentId && m.UserId == userId);

        if (metadata == null)
        {
            // Create new metadata
            metadata = new DocumentMetadata
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                DocumentId = documentId,
                Title = title,
                Authors = authors,
                Abstract = abstractText,
                Keywords = keywords,
                PublicationDate = publicationDate,
                Publisher = publisher,
                DOI = doi,
                CreatedAt = DateTime.UtcNow
            };

            _db.DocumentMetadata.Add(metadata);
            _logger.LogInformation("Creating metadata for document {DocumentId}", documentId);
        }
        else
        {
            // Update existing metadata
            metadata.Title = title;
            metadata.Authors = authors;
            metadata.Abstract = abstractText;
            metadata.Keywords = keywords;
            metadata.PublicationDate = publicationDate;
            metadata.Publisher = publisher;
            metadata.DOI = doi;
            metadata.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updating metadata for document {DocumentId}", documentId);
        }

        await _db.SaveChangesAsync();
        return metadata;
    }

    /// <summary>
    /// Get metadata for a document
    /// </summary>
    public async Task<DocumentMetadata?> GetMetadataAsync(Guid documentId, Guid userId)
    {
        return await _db.DocumentMetadata
            .FirstOrDefaultAsync(m => m.DocumentId == documentId && m.UserId == userId);
    }

    /// <summary>
    /// Get all metadata for a user's corpus
    /// </summary>
    public async Task<List<DocumentMetadata>> GetUserCorpusMetadataAsync(Guid userId)
    {
        return await _db.DocumentMetadata
            .Where(m => m.UserId == userId)
            .Include(m => m.Document)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Delete metadata for a document
    /// </summary>
    public async Task DeleteMetadataAsync(Guid documentId, Guid userId)
    {
        var metadata = await _db.DocumentMetadata
            .FirstOrDefaultAsync(m => m.DocumentId == documentId && m.UserId == userId);

        if (metadata != null)
        {
            _db.DocumentMetadata.Remove(metadata);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Deleted metadata for document {DocumentId}", documentId);
        }
    }

    /// <summary>
    /// Batch create metadata from CSV import
    /// </summary>
    public async Task<List<DocumentMetadata>> BatchCreateMetadataAsync(
        Guid userId,
        List<DocumentMetadataDto> metadataList)
    {
        var createdMetadata = new List<DocumentMetadata>();

        foreach (var dto in metadataList)
        {
            // Create a placeholder document for each metadata entry
            var document = new Document
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                FileName = $"{dto.Title}.txt", // Placeholder filename
                MimeType = "text/plain",
                FilePath = $"metadata-only/{Guid.CreateVersion7()}", // Virtual path
                FileSize = 0, // No actual file
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(document);

            // Create metadata
            var metadata = new DocumentMetadata
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                DocumentId = document.Id,
                Title = dto.Title,
                Authors = dto.Authors,
                Abstract = dto.Abstract,
                Keywords = dto.Keywords,
                PublicationDate = dto.PublicationDate,
                Publisher = dto.Venue,
                DOI = dto.DOI,
                ExtendedMetadata = new Dictionary<string, object>
                {
                    ["year"] = dto.Year ?? 0,
                    ["link"] = dto.Link ?? ""
                },
                CreatedAt = DateTime.UtcNow
            };

            _db.DocumentMetadata.Add(metadata);
            createdMetadata.Add(metadata);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Batch created {Count} metadata entries for user {UserId}", 
            createdMetadata.Count, userId);

        return createdMetadata;
    }
}

/// <summary>
/// DTO for document metadata
/// </summary>
public class DocumentMetadataDto
{
    public string Title { get; set; } = string.Empty;
    public string[] Authors { get; set; } = Array.Empty<string>();
    public string? Abstract { get; set; }
    public string[]? Keywords { get; set; }
    public DateTime? PublicationDate { get; set; }
    public int? Year { get; set; }
    public string? Venue { get; set; }
    public string? DOI { get; set; }
    public string? Link { get; set; }
}