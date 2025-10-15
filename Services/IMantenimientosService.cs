using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public interface IMantenimientosService
{
    Task<Mantenimiento> ProgramarAsync(int activoId, TipoMantenimiento tipo, DateTime fechaProgramada, string descripcion, string? tecnicoId, decimal? costo, CancellationToken cancellationToken = default);
    Task EjecutarAsync(int mantenimientoId, DateTime fechaEjecucion, string descripcionFinal, string? repuestos, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mantenimiento>> ListarCalendarioAsync(DateTime inicio, DateTime fin, CancellationToken cancellationToken = default);
}
