using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Services;

public class AlertasService(AppDbContext context, IContratosService contratosService, ILogger<AlertasService> logger) : IAlertasService
{
    public async Task GenerarAlertasVencimientosAsync(CancellationToken cancellationToken = default)
    {
        var limite = DateTime.UtcNow.Date.AddDays(30);
        var contratos = await contratosService.VencenAntesDeAsync(limite, cancellationToken);
        foreach (var contrato in contratos)
        {
            if (!await context.Alertas.AnyAsync(a => a.Tipo == TipoAlerta.Contrato && a.EntidadId == contrato.Id, cancellationToken))
            {
                context.Alertas.Add(new Alerta
                {
                    Tipo = TipoAlerta.Contrato,
                    EntidadId = contrato.Id,
                    FechaEvento = contrato.FechaFin,
                    FechaAviso = DateTime.UtcNow,
                    Mensaje = $"Contrato {contrato.NumeroContrato} vence el {contrato.FechaFin:dd/MM/yyyy}",
                    Criticidad = CriticidadAlerta.Alta
                });
            }
        }

        var mantenimientos = await context.Mantenimientos
            .Where(m => m.Estado != EstadoMantenimiento.Completado && m.FechaProgramada <= limite)
            .ToListAsync(cancellationToken);

        foreach (var mantenimiento in mantenimientos)
        {
            if (!await context.Alertas.AnyAsync(a => a.Tipo == TipoAlerta.Mantenimiento && a.EntidadId == mantenimiento.Id, cancellationToken))
            {
                context.Alertas.Add(new Alerta
                {
                    Tipo = TipoAlerta.Mantenimiento,
                    EntidadId = mantenimiento.Id,
                    FechaEvento = mantenimiento.FechaProgramada,
                    FechaAviso = DateTime.UtcNow,
                    Mensaje = $"Mantenimiento programado para el {mantenimiento.FechaProgramada:dd/MM/yyyy}",
                    Criticidad = CriticidadAlerta.Media
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Alertas de vencimiento generadas");
    }

    public async Task<IReadOnlyList<Alerta>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Alertas
            .Where(a => !a.Leida)
            .OrderBy(a => a.FechaEvento)
            .ToListAsync(cancellationToken);
    }

    public async Task MarcarLeidaAsync(int alertaId, CancellationToken cancellationToken = default)
    {
        var alerta = await context.Alertas.FindAsync([alertaId], cancellationToken);
        if (alerta is null)
        {
            return;
        }

        alerta.Leida = true;
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Alerta {AlertaId} marcada como leída", alertaId);
    }
}
