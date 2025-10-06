using Microsoft.Extensions.Options;

namespace SGI_INVENTARIO.Services;

public class FileStorageLocal : IFileStorage
{
    private readonly string _contractsPath;
    private readonly IWebHostEnvironment _environment;

    public FileStorageLocal(IWebHostEnvironment environment, IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _contractsPath = Path.Combine(environment.WebRootPath, options.Value.ContractsPath ?? "uploads/contracts");
        Directory.CreateDirectory(_contractsPath);
    }

    public async Task<string> GuardarContratoPdfAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        var path = Path.Combine(_contractsPath, safeName);

        await using var file = File.Create(path);
        await fileStream.CopyToAsync(file, cancellationToken);

        var relative = Path.GetRelativePath(_environment.WebRootPath, path).Replace("\\", "/");
        return relative;
    }

    public Task<Stream?> ObtenerStreamAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_environment.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }
}

public class FileStorageOptions
{
    public string? ContractsPath { get; set; }
}
