using ERP.Application.Options;
using ERP.Application.Services.Document;
using Microsoft.Extensions.Options;

namespace ERP.API.Services;

public sealed class DocumentStorageService(IWebHostEnvironment environment, IOptions<DocumentSettings> options) : IDocumentStorageService
{
    private readonly DocumentSettings _settings = options.Value;

    public async Task<string> SaveAsync(Stream content, string fileExtension, string referenceType, int referenceId, CancellationToken ct = default)
    {
        var relativeDirectory = Path.Combine(referenceType, referenceId.ToString());
        var storageRoot = ResolveStorageRoot();
        var targetDirectory = Path.Combine(storageRoot, relativeDirectory);
        Directory.CreateDirectory(targetDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var fullPath = Path.Combine(targetDirectory, storedFileName);

        await using (var output = File.Create(fullPath))
        {
            await content.CopyToAsync(output, ct);
        }

        return Path.Combine(relativeDirectory, storedFileName).Replace('\\', '/');
    }

    public Task<Stream> OpenReadStreamAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolveFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Stored document file was not found.", fullPath);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolveFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveStorageRoot()
    {
        var configuredPath = string.IsNullOrWhiteSpace(_settings.StorageDirectory)
            ? "App_Data/uploads/documents"
            : _settings.StorageDirectory;

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }

    private string ResolveFullPath(string storagePath)
    {
        var storageRoot = Path.GetFullPath(ResolveStorageRoot());
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, storagePath.Replace('/', Path.DirectorySeparatorChar)));

        if (!fullPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved document path is outside the storage root.");
        }

        return fullPath;
    }
}
