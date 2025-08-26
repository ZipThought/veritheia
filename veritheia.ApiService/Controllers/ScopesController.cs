using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Knowledge Database API - Scope management
/// MVP 1.4: Knowledge Scoping
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScopesController : ControllerBase
{
    private readonly VeritheiaDbContext _db;
    
    public ScopesController(VeritheiaDbContext dbContext)
    {
        _db = dbContext;
    }
    
    /// <summary>
    /// Create a knowledge scope
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateScope([FromBody] CreateScopeRequest request)
    {
        var scope = new KnowledgeScope
        {
            Id = Guid.CreateVersion7(),
            UserId = request.UserId, // Required for partition enforcement
            Name = request.Name,
            Type = request.Type ?? "General",
            CreatedAt = DateTime.UtcNow
        };
        
        _db.KnowledgeScopes.Add(scope);
        await _db.SaveChangesAsync();
        
        return Ok(new
        {
            scope.Id,
            scope.Name,
            scope.Type,
            scope.CreatedAt
        });
    }
    
    /// <summary>
    /// Get user's scopes
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetScopes([FromQuery] Guid userId)
    {
        var scopes = await _db.KnowledgeScopes
            .Where(s => s.UserId == userId) // Partition enforcement
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Type,
                DocumentCount = s.Documents.Count(),
                s.CreatedAt
            })
            .ToListAsync();
        
        return Ok(scopes);
    }
    
    /// <summary>
    /// Update scope
    /// </summary>
    [HttpPut("{scopeId}")]
    public async Task<IActionResult> UpdateScope(
        Guid scopeId,
        [FromBody] UpdateScopeRequest request,
        [FromQuery] Guid userId)
    {
        var scope = await _db.KnowledgeScopes
            .Where(s => s.UserId == userId) // Partition enforcement
            .FirstOrDefaultAsync(s => s.Id == scopeId);
        
        if (scope == null)
            return NotFound();
        
        if (!string.IsNullOrEmpty(request.Name))
            scope.Name = request.Name;
        
        await _db.SaveChangesAsync();
        
        return Ok();
    }
    
    /// <summary>
    /// Delete scope (documents remain but are unscoped)
    /// </summary>
    [HttpDelete("{scopeId}")]
    public async Task<IActionResult> DeleteScope(Guid scopeId, [FromQuery] Guid userId)
    {
        var scope = await _db.KnowledgeScopes
            .Where(s => s.UserId == userId) // Partition enforcement
            .FirstOrDefaultAsync(s => s.Id == scopeId);
        
        if (scope == null)
            return NotFound();
        
        // Documents will have ScopeId set to null due to SET NULL on delete
        _db.KnowledgeScopes.Remove(scope);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
    
    /// <summary>
    /// Assign documents to scope
    /// </summary>
    [HttpPost("{scopeId}/documents")]
    public async Task<IActionResult> AssignDocumentsToScope(
        Guid scopeId,
        [FromBody] AssignDocumentsRequest request,
        [FromQuery] Guid userId)
    {
        var scope = await _db.KnowledgeScopes
            .FirstOrDefaultAsync(s => s.Id == scopeId );
        
        if (scope == null)
            return NotFound();
        
        var documents = await _db.Documents
            .Where(d => request.DocumentIds.Contains(d.Id) )
            .ToListAsync();
        
        foreach (var doc in documents)
        {
            doc.ScopeId = scopeId;
        }
        
        await _db.SaveChangesAsync();
        
        return Ok(new
        {
            ScopeId = scopeId,
            AssignedCount = documents.Count
        });
    }
    
    public class CreateScopeRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
    }
    
    public class UpdateScopeRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
    
    public class AssignDocumentsRequest
    {
        public Guid[] DocumentIds { get; set; } = Array.Empty<Guid>();
    }
}