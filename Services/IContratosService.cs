using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public interface IContratosService
{
    Task<IReadOnlyList<Contrato>> ListarAsync(int? activoId = null, CancellationToken cancellationToken = default);
    Task<Contrato?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Contrato> SubirPdfAsync(int? activoId, string proveedor, string numeroContrato, DateTime fechaInicio, DateTime fechaFin, TipoContrato tipo, IFormFile archivo, string userId, CancellationToken cancellationToken = default);
    Task<FileStreamResult?> DescargarAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contrato>> VencenAntesDeAsync(DateTime limite, CancellationToken cancellationToken = default);
}
