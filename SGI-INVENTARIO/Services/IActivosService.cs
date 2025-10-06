using SGI_INVENTARIO.Data.Entities;
using SGI_INVENTARIO.Services.Models;

namespace SGI_INVENTARIO.Services;

public interface IActivosService
{
    Task<PaginatedResult<Activo>> BuscarAsync(string? filtro, EstadoActivo? estado, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Activo?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Activo> CrearAsync(Activo activo, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Activo activo, CancellationToken cancellationToken = default);
    Task CambiarEstadoAsync(int id, EstadoActivo estado, CancellationToken cancellationToken = default);
    Task<bool> PuedeAsignarseAsync(int id, CancellationToken cancellationToken = default);
}
