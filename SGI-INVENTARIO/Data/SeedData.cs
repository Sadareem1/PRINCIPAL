using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SGI_INVENTARIO.Data.Entities;

namespace SGI_INVENTARIO.Data;

public static class SeedData
{
    private static readonly string[] Roles = ["Administrador", "Tecnico", "Consulta"];

    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var context = scopedServices.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    logger.LogError("Error creando rol {Role}: {Errors}", role, string.Join(",", result.Errors.Select(e => e.Description)));
                }
            }
        }

        var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = "admin@sgi.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Nombres = "Administrador",
                Apellidos = "General",
                Rol = "Administrador",
                Estado = true,
                FechaAlta = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "P@ssw0rd!");
            if (!result.Succeeded)
            {
                logger.LogError("Error creando usuario admin: {Errors}", string.Join(",", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Administrador"))
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
        }

        if (!context.Areas.Any())
        {
            context.Areas.AddRange(
                new Area { Nombre = "Tecnología" },
                new Area { Nombre = "Operaciones" },
                new Area { Nombre = "Recursos Humanos" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Plantas.Any())
        {
            context.Plantas.AddRange(
                new Planta { Nombre = "Planta Principal" },
                new Planta { Nombre = "Planta Norte" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Activos.Any())
        {
            var area = context.Areas.First();
            var planta = context.Plantas.First();
            context.Activos.AddRange(
                new Activo
                {
                    CodigoInventario = "ACT-001",
                    Marca = "Dell",
                    Modelo = "Latitude 5420",
                    NumeroSerie = "DL5420-001",
                    Categoria = "Laptop",
                    Ubicacion = "Oficina 101",
                    Area = area,
                    Planta = planta,
                    Estado = EstadoActivo.Operativo,
                    FechaRegistro = DateTime.UtcNow,
                    ActivoEnSistema = true
                },
                new Activo
                {
                    CodigoInventario = "ACT-002",
                    Marca = "HP",
                    Modelo = "LaserJet Pro",
                    NumeroSerie = "HP-LJ-0001",
                    Categoria = "Impresora",
                    Ubicacion = "Sala de reuniones",
                    Area = area,
                    Planta = planta,
                    Estado = EstadoActivo.Asignado,
                    FechaRegistro = DateTime.UtcNow,
                    ActivoEnSistema = true
                },
                new Activo
                {
                    CodigoInventario = "ACT-003",
                    Marca = "Cisco",
                    Modelo = "Catalyst 9300",
                    NumeroSerie = "CSC9300-01",
                    Categoria = "Switch",
                    Ubicacion = "Data Center",
                    Area = area,
                    Planta = planta,
                    Estado = EstadoActivo.Mantenimiento,
                    FechaRegistro = DateTime.UtcNow,
                    ActivoEnSistema = true
                }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Contratos.Any())
        {
            context.Contratos.Add(new Contrato
            {
                ActivoId = await context.Activos.Select(a => a.Id).FirstAsync(),
                Proveedor = "Proveedor Demo",
                NumeroContrato = "CTR-0001",
                FechaInicio = DateTime.UtcNow.Date.AddMonths(-3),
                FechaFin = DateTime.UtcNow.Date.AddMonths(3),
                Tipo = TipoContrato.Garantia,
                ArchivoNombre = "dummy.pdf",
                ArchivoRuta = "uploads/contracts/dummy.pdf",
                ContentType = "application/pdf",
                TamañoBytes = 1024,
                SubidoPorUserId = admin.Id,
                FechaSubida = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
