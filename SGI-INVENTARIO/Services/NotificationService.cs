using System.Collections.Concurrent;

namespace SGI_INVENTARIO.Services;

public class NotificationService : INotificationService
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _notifications = new();

    public Task CrearNotificacionAsync(string usuarioId, string mensaje, CancellationToken cancellationToken = default)
    {
        var queue = _notifications.GetOrAdd(usuarioId, _ => new ConcurrentQueue<string>());
        queue.Enqueue(mensaje);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ObtenerNotificacionesAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        if (_notifications.TryGetValue(usuarioId, out var queue))
        {
            return Task.FromResult<IReadOnlyList<string>>(queue.ToArray());
        }

        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
