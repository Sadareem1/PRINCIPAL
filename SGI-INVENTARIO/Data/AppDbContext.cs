using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<Mantenimiento> Mantenimientos => Set<Mantenimiento>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Planta> Plantas => Set<Planta>();
    public DbSet<Alerta> Alertas => Set<Alerta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Activo>()
            .HasIndex(a => a.CodigoInventario)
            .IsUnique();

        builder.Entity<Contrato>()
            .HasOne(c => c.Activo)
            .WithMany(a => a.Contratos)
            .HasForeignKey(c => c.ActivoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Asignacion>()
            .HasOne(a => a.Activo)
            .WithMany(a => a.Asignaciones)
            .HasForeignKey(a => a.ActivoId);

        builder.Entity<Mantenimiento>()
            .HasOne(m => m.Activo)
            .WithMany(a => a.Mantenimientos)
            .HasForeignKey(m => m.ActivoId);

        builder.Entity<Solicitud>()
            .HasOne(s => s.Activo)
            .WithMany()
            .HasForeignKey(s => s.ActivoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
