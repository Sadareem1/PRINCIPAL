using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Mantenimiento
{
    public int Id { get; set; }

    [Required]
    public int ActivoId { get; set; }
    public Activo Activo { get; set; } = default!;

    [Required]
    public TipoMantenimiento Tipo { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaProgramada { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaEjecucion { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = default!;

    public string? RepuestosUsados { get; set; }

    [StringLength(450)]
    public string? TecnicoId { get; set; }

    public decimal? Costo { get; set; }

    public EstadoMantenimiento Estado { get; set; } = EstadoMantenimiento.Programado;
}
