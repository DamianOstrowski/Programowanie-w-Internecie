namespace HelpDeskHero.Api.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public LocalFileStorage(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<StoredFileResult> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        var root = ResolveRootPath();

        Directory.CreateDirectory(root);

        var extension = Path.GetExtension(file.FileName);
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(root, safeName);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, ct);

        return new StoredFileResult
        {
            OriginalFileName = file.FileName,
            StoredFileName = safeName,
            RelativePath = safeName,
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var root = ResolveRootPath();

        var path = Path.Combine(root, relativePath);
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var root = ResolveRootPath();

        var path = Path.Combine(root, relativePath);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string ResolveRootPath()
    {
        var configured = _configuration["FileStorage:RootPath"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        return Path.Combine(_environment.ContentRootPath, "App_Data", "attachments");
    }
}
