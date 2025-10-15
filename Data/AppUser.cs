using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SGI_INVENTARIO.Data;

public class AppUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Apellidos { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Dni { get; set; }

    [StringLength(20)]
    public string? Celular { get; set; }

    [StringLength(100)]
    public string? Rol { get; set; }

    public bool Estado { get; set; } = true;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
}
