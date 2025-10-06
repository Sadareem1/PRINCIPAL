using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Solicitud
{
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public string SolicitanteId { get; set; } = default!;

    public TipoSolicitud Tipo { get; set; }

    public Prioridad Prioridad { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Abierta;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = default!;

    public int? ActivoId { get; set; }
    public Activo? Activo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaCierre { get; set; }
}
