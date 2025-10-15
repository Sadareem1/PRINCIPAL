using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public class MantenimientosService(AppDbContext context, ILogger<MantenimientosService> logger) : IMantenimientosService
{
    public async Task<Mantenimiento> ProgramarAsync(int activoId, TipoMantenimiento tipo, DateTime fechaProgramada, string descripcion, string? tecnicoId, decimal? costo, CancellationToken cancellationToken = default)
    {
        var mantenimiento = new Mantenimiento
        {
            ActivoId = activoId,
            Tipo = tipo,
            FechaProgramada = fechaProgramada,
            Descripcion = descripcion,
            TecnicoId = tecnicoId,
            Costo = costo,
            Estado = EstadoMantenimiento.Programado
        };

        context.Mantenimientos.Add(mantenimiento);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Mantenimiento {MantenimientoId} programado para activo {ActivoId}", mantenimiento.Id, activoId);
        return mantenimiento;
    }

    public async Task EjecutarAsync(int mantenimientoId, DateTime fechaEjecucion, string descripcionFinal, string? repuestos, CancellationToken cancellationToken = default)
    {
        var mantenimiento = await context.Mantenimientos.FindAsync([mantenimientoId], cancellationToken);
        if (mantenimiento is null)
        {
            throw new InvalidOperationException("No se encontró el mantenimiento indicado.");
        }

        if (mantenimiento.Estado == EstadoMantenimiento.Completado)
        {
            return;
        }

        mantenimiento.FechaEjecucion = fechaEjecucion;
        mantenimiento.Descripcion = descripcionFinal;
        mantenimiento.RepuestosUsados = repuestos;
        mantenimiento.Estado = EstadoMantenimiento.Completado;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Mantenimiento {MantenimientoId} marcado como completado", mantenimientoId);
    }

    public async Task<IReadOnlyList<Mantenimiento>> ListarCalendarioAsync(DateTime inicio, DateTime fin, CancellationToken cancellationToken = default)
    {
        return await context.Mantenimientos
            .Include(m => m.Activo)
            .Where(m => m.FechaProgramada >= inicio && m.FechaProgramada <= fin)
            .OrderBy(m => m.FechaProgramada)
            .ToListAsync(cancellationToken);
    }
}
