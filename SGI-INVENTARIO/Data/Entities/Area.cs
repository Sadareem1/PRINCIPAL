using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data.Entities;

public class Area
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = default!;

    public bool Activo { get; set; } = true;

    public ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
