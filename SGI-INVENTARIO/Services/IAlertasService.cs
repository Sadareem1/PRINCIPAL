using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public interface IAlertasService
{
    Task GenerarAlertasVencimientosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alerta>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);
    Task MarcarLeidaAsync(int alertaId, CancellationToken cancellationToken = default);
}
