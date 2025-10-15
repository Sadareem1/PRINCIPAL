namespace SGI_INVENTARIO.Services;

public interface INotificationService
{
    Task CrearNotificacionAsync(string usuarioId, string mensaje, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ObtenerNotificacionesAsync(string usuarioId, CancellationToken cancellationToken = default);
}
