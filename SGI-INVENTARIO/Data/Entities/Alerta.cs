using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Alerta
{
    public int Id { get; set; }

    public TipoAlerta Tipo { get; set; }

    public int EntidadId { get; set; }

    public DateTime FechaEvento { get; set; }

    public DateTime FechaAviso { get; set; }

    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; } = default!;

    public bool Leida { get; set; }

    public CriticidadAlerta Criticidad { get; set; } = CriticidadAlerta.Media;
}
