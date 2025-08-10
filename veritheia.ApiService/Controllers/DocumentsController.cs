using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veritheia.Core.Services;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Knowledge Database API - Document management
/// Post-DDD: Controller uses services directly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly VeritheiaDbContext _db;
    
    public DocumentsController(
        DocumentService documentService,
        VeritheiaDbContext dbContext)
    {
        _documentService = documentService;
        _db = dbContext;
    }
    
    /// <summary>
    /// Upload a document to the knowledge base
    /// MVP 1.3.1: Artifact & Metadata API - Create
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] Guid userId,
        [FromForm] Guid? scopeId = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");
        
        using var stream = file.OpenReadStream();
        var document = await _documentService.UploadDocumentAsync(
            stream,
            file.FileName,
            file.ContentType,
            userId,
            scopeId);
        
        return Ok(new
        {
            document.Id,
            document.FileName,
            document.FilePath,
            document.FileSize,
            document.UploadedAt
        });
    }
    
    /// <summary>
    /// Get user's documents
    /// MVP 1.3.1: Artifact & Metadata API - Read
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] Guid userId)
    {
        var documents = await _documentService.GetUserDocumentsAsync(userId);
        
        return Ok(documents.Select(d => new
        {
            d.Id,
            d.FileName,
            d.MimeType,
            d.FileSize,
            d.UploadedAt,
            Scope = d.Scope?.Name
        }));
    }
    
    /// <summary>
    /// Get document content
    /// MVP 1.3.1: Artifact & Metadata API - Read content
    /// </summary>
    [HttpGet("{documentId}/content")]
    public async Task<IActionResult> GetDocumentContent(Guid documentId, [FromQuery] Guid userId)
    {
        try
        {
            var stream = await _documentService.GetDocumentContentAsync(documentId, userId);
            
            var document = await _db.Documents.FindAsync(documentId);
            if (document == null)
                return NotFound();
            
            return File(stream, document.MimeType, document.FileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    
    /// <summary>
    /// Delete document
    /// MVP 1.3.1: Artifact & Metadata API - Delete
    /// </summary>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId, [FromQuery] Guid userId)
    {
        try
        {
            await _documentService.DeleteDocumentAsync(documentId, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    
    /// <summary>
    /// Update document metadata
    /// MVP 1.3.1: Artifact & Metadata API - Update
    /// </summary>
    [HttpPatch("{documentId}/metadata")]
    public async Task<IActionResult> UpdateMetadata(
        Guid documentId,
        [FromBody] UpdateMetadataRequest request,
        [FromQuery] Guid userId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);
        
        if (document == null)
            return NotFound();
        
        if (request.ScopeId.HasValue)
            document.ScopeId = request.ScopeId.Value;
        
        if (!string.IsNullOrEmpty(request.FileName))
            document.FileName = request.FileName;
        
        await _db.SaveChangesAsync();
        
        return Ok();
    }
    
    public class UpdateMetadataRequest
    {
        public Guid? ScopeId { get; set; }
        public string? FileName { get; set; }
    }
}