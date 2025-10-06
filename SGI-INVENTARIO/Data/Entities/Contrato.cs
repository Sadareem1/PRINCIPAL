using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Contrato
{
    public int Id { get; set; }

    public int? ActivoId { get; set; }
    public Activo? Activo { get; set; }

    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    [StringLength(200)]
    public string Proveedor { get; set; } = default!;

    [Required(ErrorMessage = "El número de contrato es obligatorio.")]
    [StringLength(100)]
    public string NumeroContrato { get; set; } = default!;

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    [Required]
    public TipoContrato Tipo { get; set; }

    [StringLength(260)]
    public string? ArchivoNombre { get; set; }

    [StringLength(500)]
    public string? ArchivoRuta { get; set; }

    [StringLength(150)]
    public string? ContentType { get; set; }

    public long TamañoBytes { get; set; }

    [StringLength(450)]
    public string? SubidoPorUserId { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public bool Eliminado { get; set; }
}
