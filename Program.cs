using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

using SGI_INVENTARIO.Components;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Logging;
using SGI_INVENTARIO.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var logFile = Path.Combine(builder.Environment.ContentRootPath, "Logs", "sgi.log");
builder.Logging.AddProvider(new FileLoggerProvider(logFile));

// ---------- Options
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<JobSchedulerOptions>(builder.Configuration.GetSection("JobScheduler"));

// ---------- EF Core + Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Sin confirmación de email por ahora
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Usa la UI por defecto de Identity (RCL)

// ---------- Autorización (políticas)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("GestionTecnica", policy => policy.RequireRole("Administrador", "Tecnico"));
    options.AddPolicy("SoloLectura", policy => policy.RequireRole("Administrador", "Tecnico", "Consulta"));
    options.AddPolicy("ContratosDownload", policy => policy.RequireRole("Administrador", "Tecnico"));
});

// ---------- Razor/Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

// Permitir que Identity (Razor Class Library) encuentre parciales en Components/Pages/Shared
builder.Services.Configure<RazorViewEngineOptions>(o =>
{
    // Razor Pages normales
    o.PageViewLocationFormats.Insert(0, "/Components/Pages/{0}.cshtml");
    o.PageViewLocationFormats.Insert(0, "/Components/Pages/Shared/{0}.cshtml");

    // Búsqueda en áreas (la UI por defecto de Identity es un RCL/área)
    o.AreaPageViewLocationFormats.Insert(0, "/Components/Pages/{0}.cshtml");
    o.AreaPageViewLocationFormats.Insert(0, "/Components/Pages/Shared/{0}.cshtml");

    // Compatibilidad adicional con MVC Views si algo lo usa
    o.ViewLocationFormats.Insert(0, "/Components/Pages/{0}.cshtml");
    o.ViewLocationFormats.Insert(0, "/Components/Pages/Shared/{0}.cshtml");
});

// ---------- DI aplicación
builder.Services.AddScoped<IActivosService, ActivosService>();
builder.Services.AddScoped<IContratosService, ContratosService>();
builder.Services.AddScoped<IAsignacionesService, AsignacionesService>();
builder.Services.AddScoped<IMantenimientosService, MantenimientosService>();
builder.Services.AddScoped<ISolicitudesService, SolicitudesService>();
builder.Services.AddScoped<IAlertasService, AlertasService>();

// Tu servicio propio de notificaciones (interface + clase de tu proyecto)
builder.Services.AddSingleton<SGI_INVENTARIO.Services.INotificationService, SGI_INVENTARIO.Services.NotificationService>();

builder.Services.AddScoped<IFileStorage, FileStorageLocal>();
builder.Services.AddHostedService<AlertWorker>();

// ---------- Radzen (servicios necesarios para RadzenDialog/Notification/etc.)
// Nota: nombres totalmente calificados para NO colisionar con tu NotificationService
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();   // <- distinto namespace que el tuyo
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();
builder.Services.AddScoped<Radzen.ThemeService>();

var app = builder.Build();

// ---------- Seed inicial
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
    await SeedData.InitializeAsync(scope.ServiceProvider, logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Orden correcto
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Antiforgery (requerido por Razor Components/Pages con [ValidateAntiForgeryToken])
app.UseAntiforgery();

// Bloquear acceso directo a /uploads/contracts (descarga solo por endpoint autorizado)
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/uploads/contracts", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});

app.UseStaticFiles();

app.MapRazorPages();

// Endpoint seguro para descargar contratos
app.MapGet("/contratos/descargar/{id:int}", async Task<IResult> (int id, IContratosService contratosService) =>
{
    var download = await contratosService.DescargarAsync(id);
    if (download is null) return Results.NotFound();

    return Results.Stream(download.FileStream, download.ContentType ?? "application/pdf", download.FileDownloadName);
})
.RequireAuthorization("ContratosDownload");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
