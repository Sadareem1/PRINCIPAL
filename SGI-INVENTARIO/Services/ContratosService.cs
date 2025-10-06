using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public class ContratosService(AppDbContext context, IFileStorage storage, ILogger<ContratosService> logger) : IContratosService
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    public async Task<IReadOnlyList<Contrato>> ListarAsync(int? activoId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Contratos.Include(c => c.Activo).Where(c => !c.Eliminado);
        if (activoId.HasValue)
        {
            query = query.Where(c => c.ActivoId == activoId);
        }
        return await query.OrderByDescending(c => c.FechaFin).ToListAsync(cancellationToken);
    }

    public async Task<Contrato?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Contratos.Include(c => c.Activo).FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado, cancellationToken);
    }

    public async Task<Contrato> SubirPdfAsync(int? activoId, string proveedor, string numeroContrato, DateTime fechaInicio, DateTime fechaFin, TipoContrato tipo, IFormFile archivo, string userId, CancellationToken cancellationToken = default)
    {
        if (archivo is null || archivo.Length == 0)
        {
            throw new ArgumentException("Debe seleccionar un archivo PDF.", nameof(archivo));
        }

        if (!string.Equals(archivo.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Solo se permiten archivos PDF.");
        }

        if (archivo.Length > MaxFileSize)
        {
            throw new InvalidOperationException("El archivo supera el tamaño permitido de 20MB.");
        }

        var contrato = new Contrato
        {
            ActivoId = activoId,
            Proveedor = proveedor,
            NumeroContrato = numeroContrato,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Tipo = tipo,
            SubidoPorUserId = userId,
            FechaSubida = DateTime.UtcNow,
            TamañoBytes = archivo.Length,
            ContentType = archivo.ContentType
        };

        await context.Contratos.AddAsync(contrato, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var safeBaseName = Regex.Replace(Path.GetFileNameWithoutExtension(archivo.FileName), "[^a-zA-Z0-9_-]", "-");
        var safeFileName = $"{contrato.Id}_{DateTime.UtcNow:yyyyMMddHHmm}_{safeBaseName}.pdf";

        await using var stream = archivo.OpenReadStream();
        var relativePath = await storage.GuardarContratoPdfAsync(stream, safeFileName, cancellationToken);

        contrato.ArchivoNombre = safeFileName;
        contrato.ArchivoRuta = relativePath;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Contrato {ContratoId} subido por {UserId}", contrato.Id, userId);

        return contrato;
    }

    public async Task<FileStreamResult?> DescargarAsync(int id, CancellationToken cancellationToken = default)
    {
        var contrato = await ObtenerPorIdAsync(id, cancellationToken);
        if (contrato is null || string.IsNullOrEmpty(contrato.ArchivoRuta))
        {
            return null;
        }

        var stream = await storage.ObtenerStreamAsync(contrato.ArchivoRuta, cancellationToken);
        if (stream is null)
        {
            return null;
        }

        logger.LogInformation("Contrato {ContratoId} descargado", id);
        return new FileStreamResult(stream, contrato.ContentType ?? "application/pdf")
        {
            FileDownloadName = contrato.ArchivoNombre ?? "contrato.pdf"
        };
    }

    public async Task<IReadOnlyList<Contrato>> VencenAntesDeAsync(DateTime limite, CancellationToken cancellationToken = default)
    {
        return await context.Contratos
            .Where(c => !c.Eliminado && c.FechaFin <= limite)
            .OrderBy(c => c.FechaFin)
            .ToListAsync(cancellationToken);
    }
}
