using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veritheia.Data;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Knowledge Database API - Search endpoints
/// Direct EF Core queries with PostgreSQL full-text and vector search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly VeritheiaDbContext _db;
    
    public SearchController(VeritheiaDbContext dbContext)
    {
        _db = dbContext;
    }
    
    /// <summary>
    /// Full-text search across documents
    /// MVP 1.3.2: Full-Text Search API
    /// </summary>
    [HttpGet("text")]
    public async Task<IActionResult> SearchText(
        [FromQuery] string query,
        [FromQuery] Guid userId,
        [FromQuery] Guid? scopeId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required");
        
        var documentsQuery = _db.Documents
            .Where(d => d.UserId == userId);
        
        // Apply scope filter if provided
        if (scopeId.HasValue)
            documentsQuery = documentsQuery.Where(d => d.ScopeId == scopeId.Value);
        
        // Simple text search (can be enhanced with PostgreSQL full-text search)
        documentsQuery = documentsQuery.Where(d => 
            d.FileName.Contains(query) ||
            d.Metadata != null && d.Metadata.Title.Contains(query));
        
        var totalCount = await documentsQuery.CountAsync();
        
        var documents = await documentsQuery
            .OrderByDescending(d => d.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.FileName,
                d.MimeType,
                d.FileSize,
                d.UploadedAt,
                Metadata = d.Metadata != null ? new
                {
                    d.Metadata.Title,
                    d.Metadata.Authors,
                    d.Metadata.PublicationDate
                } : null
            })
            .ToListAsync();
        
        return Ok(new
        {
            Results = documents,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
    
    /// <summary>
    /// Semantic search using vector similarity
    /// MVP 1.3.3: Semantic Search API
    /// </summary>
    [HttpPost("semantic")]
    public async Task<IActionResult> SearchSemantic(
        [FromBody] SemanticSearchRequest request)
    {
        if (request.Embedding == null || request.Embedding.Length == 0)
            return BadRequest("Embedding is required");
        
        // This would use pgvector for similarity search
        // For now, returning journey-scoped segments as example
        var segments = await _db.JourneyDocumentSegments
            .Where(s => s.JourneyId == request.JourneyId)
            .Include(s => s.Document)
            .Take(request.Limit)
            .Select(s => new
            {
                s.Id,
                s.DocumentId,
                DocumentName = s.Document.FileName,
                s.SegmentContent,
                s.SegmentType,
                s.SequenceIndex
            })
            .ToListAsync();
        
        return Ok(new
        {
            Results = segments,
            QueryEmbeddingDimensions = request.Embedding.Length,
            Threshold = request.Threshold
        });
    }
    
    /// <summary>
    /// Scoped query combining filters
    /// MVP 1.3.4: Scoped Query API
    /// </summary>
    [HttpGet("scoped")]
    public async Task<IActionResult> SearchScoped(
        [FromQuery] Guid userId,
        [FromQuery] Guid? scopeId = null,
        [FromQuery] Guid? journeyId = null,
        [FromQuery] string? documentType = null,
        [FromQuery] DateTime? since = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Start with user's documents
        var query = _db.Documents.Where(d => d.UserId == userId);
        
        // Apply scope filter
        if (scopeId.HasValue)
            query = query.Where(d => d.ScopeId == scopeId.Value);
        
        // Apply document type filter
        if (!string.IsNullOrEmpty(documentType))
            query = query.Where(d => d.MimeType.Contains(documentType));
        
        // Apply date filter
        if (since.HasValue)
            query = query.Where(d => d.UploadedAt >= since.Value);
        
        var totalCount = await query.CountAsync();
        
        var results = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.FileName,
                d.MimeType,
                d.FileSize,
                d.UploadedAt,
                d.ScopeId,
                HasJourneyProjections = journeyId.HasValue ? 
                    d.JourneySegments.Any(s => s.JourneyId == journeyId.Value) : 
                    false
            })
            .ToListAsync();
        
        return Ok(new
        {
            Results = results,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Filters = new
            {
                UserId = userId,
                ScopeId = scopeId,
                JourneyId = journeyId,
                DocumentType = documentType,
                Since = since
            }
        });
    }
    
    public class SemanticSearchRequest
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public Guid JourneyId { get; set; }
        public double Threshold { get; set; } = 0.7;
        public int Limit { get; set; } = 10;
    }
}