using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Asignacion
{
    public int Id { get; set; }

    [Required]
    public int ActivoId { get; set; }
    public Activo Activo { get; set; } = default!;

    [Required]
    [StringLength(450)]
    public string UsuarioId { get; set; } = default!;

    public int? AreaId { get; set; }
    public Area? Area { get; set; }

    public int? PlantaId { get; set; }
    public Planta? Planta { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaEntrega { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaDevolucion { get; set; }

    [StringLength(200)]
    public string ResponsableEntrega { get; set; } = default!;

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public bool Activa => !FechaDevolucion.HasValue;
}
