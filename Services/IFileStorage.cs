namespace SGI_INVENTARIO.Services;

public interface IFileStorage
{
    Task<string> GuardarContratoPdfAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream?> ObtenerStreamAsync(string relativePath, CancellationToken cancellationToken = default);
}
