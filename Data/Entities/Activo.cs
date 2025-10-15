using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Activo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de inventario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El código de inventario no puede superar los 100 caracteres.")]
    public string CodigoInventario { get; set; } = default!;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(100)]
    public string Marca { get; set; } = default!;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(150)]
    public string Modelo { get; set; } = default!;

    [StringLength(150)]
    public string? NumeroSerie { get; set; }

    [StringLength(150)]
    public string? Categoria { get; set; }

    [StringLength(150)]
    public string? Ubicacion { get; set; }

    public int? AreaId { get; set; }
    public Area? Area { get; set; }

    public int? PlantaId { get; set; }
    public Planta? Planta { get; set; }

    [Required]
    public EstadoActivo Estado { get; set; } = EstadoActivo.Operativo;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool ActivoEnSistema { get; set; } = true;

    [StringLength(500)]
    public string? Notas { get; set; }

    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    public ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();
}
