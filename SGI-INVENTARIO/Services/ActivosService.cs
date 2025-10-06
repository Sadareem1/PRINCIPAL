using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;
using SGI_INVENTARIO.Services.Models;

namespace SGI_INVENTARIO.Services;

public class ActivosService(AppDbContext context, ILogger<ActivosService> logger) : IActivosService
{
    public async Task<PaginatedResult<Activo>> BuscarAsync(string? filtro, EstadoActivo? estado, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Activos
            .Include(a => a.Area)
            .Include(a => a.Planta)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(a => a.CodigoInventario.Contains(filtro) || a.Marca.Contains(filtro) || a.Modelo.Contains(filtro));
        }

        if (estado.HasValue)
        {
            query = query.Where(a => a.Estado == estado.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.CodigoInventario)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Activo>(items, total, page, pageSize);
    }

    public async Task<Activo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Activos
            .Include(a => a.Area)
            .Include(a => a.Planta)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Activo> CrearAsync(Activo activo, CancellationToken cancellationToken = default)
    {
        context.Activos.Add(activo);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error creando activo {Codigo}", activo.CodigoInventario);
            throw;
        }
        return activo;
    }

    public async Task ActualizarAsync(Activo activo, CancellationToken cancellationToken = default)
    {
        context.Activos.Update(activo);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error actualizando activo {Id}", activo.Id);
            throw;
        }
    }

    public async Task CambiarEstadoAsync(int id, EstadoActivo estado, CancellationToken cancellationToken = default)
    {
        var activo = await context.Activos.FindAsync([id], cancellationToken);
        if (activo is null)
        {
            throw new InvalidOperationException($"No se encontró el activo con Id {id}.");
        }

        activo.Estado = estado;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> PuedeAsignarseAsync(int id, CancellationToken cancellationToken = default)
    {
        return !await context.Asignaciones.AnyAsync(a => a.ActivoId == id && a.FechaDevolucion == null, cancellationToken);
    }
}
