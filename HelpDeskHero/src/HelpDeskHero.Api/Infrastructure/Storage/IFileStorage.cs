namespace HelpDeskHero.Api.Infrastructure.Storage;

public interface IFileStorage
{
    Task<StoredFileResult> SaveAsync(IFormFile file, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}
