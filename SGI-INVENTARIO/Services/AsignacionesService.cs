using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public class AsignacionesService(AppDbContext context, IActivosService activosService, ILogger<AsignacionesService> logger) : IAsignacionesService
{
    public async Task<IReadOnlyList<Asignacion>> ObtenerActivasAsync(CancellationToken cancellationToken = default)
    {
        return await context.Asignaciones
            .Include(a => a.Activo)
            .Where(a => a.FechaDevolucion == null)
            .OrderByDescending(a => a.FechaEntrega)
            .ToListAsync(cancellationToken);
    }

    public async Task<Asignacion> AsignarAsync(int activoId, string usuarioId, DateTime fechaEntrega, string responsableEntrega, int? areaId, int? plantaId, string? observaciones, CancellationToken cancellationToken = default)
    {
        if (!await activosService.PuedeAsignarseAsync(activoId, cancellationToken))
        {
            throw new InvalidOperationException("El activo ya se encuentra asignado y debe ser devuelto antes de una nueva asignación.");
        }

        var asignacion = new Asignacion
        {
            ActivoId = activoId,
            UsuarioId = usuarioId,
            FechaEntrega = fechaEntrega,
            ResponsableEntrega = responsableEntrega,
            AreaId = areaId,
            PlantaId = plantaId,
            Observaciones = observaciones
        };

        context.Asignaciones.Add(asignacion);
        await context.SaveChangesAsync(cancellationToken);

        var activo = await context.Activos.FindAsync([activoId], cancellationToken);
        if (activo is not null)
        {
            activo.Estado = EstadoActivo.Asignado;
            await context.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Activo {ActivoId} asignado a usuario {UsuarioId}", activoId, usuarioId);
        return asignacion;
    }

    public async Task DevolverAsync(int asignacionId, DateTime fechaDevolucion, CancellationToken cancellationToken = default)
    {
        var asignacion = await context.Asignaciones.FindAsync([asignacionId], cancellationToken);
        if (asignacion is null)
        {
            throw new InvalidOperationException("No se encontró la asignación indicada.");
        }

        if (asignacion.FechaDevolucion.HasValue)
        {
            return;
        }

        asignacion.FechaDevolucion = fechaDevolucion;
        await context.SaveChangesAsync(cancellationToken);

        var activo = await context.Activos.FindAsync([asignacion.ActivoId], cancellationToken);
        if (activo is not null && activo.Estado == EstadoActivo.Asignado)
        {
            activo.Estado = EstadoActivo.Operativo;
            await context.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Asignación {AsignacionId} devuelta", asignacionId);
    }
}
