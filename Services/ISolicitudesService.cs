using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public interface ISolicitudesService
{
    Task<Solicitud> CrearAsync(string solicitanteId, TipoSolicitud tipo, Prioridad prioridad, string descripcion, int? activoId, CancellationToken cancellationToken = default);
    Task CambiarEstadoAsync(int solicitudId, EstadoSolicitud nuevoEstado, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solicitud>> ListarAsync(string? search, EstadoSolicitud? estado, CancellationToken cancellationToken = default);
    Task<Solicitud?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
}
