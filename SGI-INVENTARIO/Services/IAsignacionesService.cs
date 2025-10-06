using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public interface IAsignacionesService
{
    Task<IReadOnlyList<Asignacion>> ObtenerActivasAsync(CancellationToken cancellationToken = default);
    Task<Asignacion> AsignarAsync(int activoId, string usuarioId, DateTime fechaEntrega, string responsableEntrega, int? areaId, int? plantaId, string? observaciones, CancellationToken cancellationToken = default);
    Task DevolverAsync(int asignacionId, DateTime fechaDevolucion, CancellationToken cancellationToken = default);
}
