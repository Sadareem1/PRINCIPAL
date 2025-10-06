using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public class SolicitudesService(AppDbContext context, ILogger<SolicitudesService> logger) : ISolicitudesService
{
    public async Task<Solicitud> CrearAsync(string solicitanteId, TipoSolicitud tipo, Prioridad prioridad, string descripcion, int? activoId, CancellationToken cancellationToken = default)
    {
        var solicitud = new Solicitud
        {
            SolicitanteId = solicitanteId,
            Tipo = tipo,
            Prioridad = prioridad,
            Descripcion = descripcion,
            ActivoId = activoId,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoSolicitud.Abierta
        };

        context.Solicitudes.Add(solicitud);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Solicitud {SolicitudId} creada por {UsuarioId}", solicitud.Id, solicitanteId);
        return solicitud;
    }

    public async Task CambiarEstadoAsync(int solicitudId, EstadoSolicitud nuevoEstado, CancellationToken cancellationToken = default)
    {
        var solicitud = await context.Solicitudes.FindAsync([solicitudId], cancellationToken);
        if (solicitud is null)
        {
            throw new InvalidOperationException("No se encontró la solicitud indicada.");
        }

        if (!EsCambioValido(solicitud.Estado, nuevoEstado))
        {
            throw new InvalidOperationException("Cambio de estado inválido para la solicitud.");
        }

        solicitud.Estado = nuevoEstado;
        if (nuevoEstado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
        {
            solicitud.FechaCierre = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Solicitud {SolicitudId} cambió a estado {Estado}", solicitudId, nuevoEstado);
    }

    public async Task<IReadOnlyList<Solicitud>> ListarAsync(string? search, EstadoSolicitud? estado, CancellationToken cancellationToken = default)
    {
        var query = context.Solicitudes.Include(s => s.Activo).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Descripcion.Contains(search));
        }

        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado);
        }

        return await query.OrderByDescending(s => s.FechaCreacion).ToListAsync(cancellationToken);
    }

    public async Task<Solicitud?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Solicitudes.Include(s => s.Activo).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    private static bool EsCambioValido(EstadoSolicitud actual, EstadoSolicitud nuevo)
    {
        return (actual, nuevo) switch
        {
            (EstadoSolicitud.Abierta, EstadoSolicitud.EnProceso) => true,
            (EstadoSolicitud.EnProceso, EstadoSolicitud.Resuelta) => true,
            (EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada) => true,
            (EstadoSolicitud.Abierta, EstadoSolicitud.Cerrada) => false,
            (EstadoSolicitud.EnProceso, EstadoSolicitud.Abierta) => false,
            _ => true
        };
    }
}
