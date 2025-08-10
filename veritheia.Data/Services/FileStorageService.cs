using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Veritheia.Core.Interfaces;

namespace Veritheia.Data.Services;

/// <summary>
/// Filesystem implementation of document storage
/// This is a true repository pattern - abstracts external storage
/// </summary>
public class FileStorageService : IDocumentStorageRepository
{
    private readonly string _rootPath;
    
    public FileStorageService(string rootPath)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Directory.CreateDirectory(_rootPath);
    }
    
    public async Task<string> StoreDocumentAsync(
        Stream content, 
        string fileName,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        // Create date-based directory structure
        var date = DateTime.UtcNow;
        var directory = Path.Combine(
            _rootPath,
            date.Year.ToString(),
            date.Month.ToString("D2"),
            date.Day.ToString("D2"));
        
        Directory.CreateDirectory(directory);
        
        // Generate unique filename
        var fileId = Guid.CreateVersion7();
        var extension = Path.GetExtension(fileName);
        var storageName = $"{fileId}{extension}";
        var fullPath = Path.Combine(directory, storageName);
        
        // Store file
        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);
        
        // Return relative path
        return Path.GetRelativePath(_rootPath, fullPath);
    }
    
    public Task<Stream> GetDocumentContentAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Document not found: {storagePath}");
        
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream>(stream);
    }
    
    public Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }
    
    public Task DeleteDocumentAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        
        return Task.CompletedTask;
    }
    
    public async Task<DocumentStorageMetadata> GetMetadataAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Document not found: {storagePath}");
        
        var fileInfo = new FileInfo(fullPath);
        
        // Calculate hash
        using var stream = File.OpenRead(fullPath);
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        var hash = Convert.ToBase64String(hashBytes);
        
        return new DocumentStorageMetadata(
            fileInfo.Length,
            fileInfo.CreationTimeUtc,
            fileInfo.LastWriteTimeUtc,
            hash);
    }
}