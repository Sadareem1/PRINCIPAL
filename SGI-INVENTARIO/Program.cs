using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SGI_INVENTARIO.Components;
using SGI_INVENTARIO.Data;
using SGI_INVENTARIO.Logging;
using SGI_INVENTARIO.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var logFile = Path.Combine(builder.Environment.ContentRootPath, "Logs", "sgi.log");
builder.Logging.AddProvider(new FileLoggerProvider(logFile));

builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<JobSchedulerOptions>(builder.Configuration.GetSection("JobScheduler"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddAuthentication().AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("GestionTecnica", policy => policy.RequireRole("Administrador", "Tecnico"));
    options.AddPolicy("SoloLectura", policy => policy.RequireRole("Administrador", "Tecnico", "Consulta"));
    options.AddPolicy("ContratosDownload", policy => policy.RequireRole("Administrador", "Tecnico"));
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

builder.Services.AddScoped<IActivosService, ActivosService>();
builder.Services.AddScoped<IContratosService, ContratosService>();
builder.Services.AddScoped<IAsignacionesService, AsignacionesService>();
builder.Services.AddScoped<IMantenimientosService, MantenimientosService>();
builder.Services.AddScoped<ISolicitudesService, SolicitudesService>();
builder.Services.AddScoped<IAlertasService, AlertasService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorage, FileStorageLocal>();
builder.Services.AddHostedService<AlertWorker>();

var app = builder.Build();

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

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/uploads/contracts", StringComparison.OrdinalIgnoreCase) &&
        !(context.User.IsInRole("Administrador") || context.User.IsInRole("Tecnico")))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/contratos/descargar/{id:int}", async Task<IResult> (int id, IContratosService contratosService) =>
{
    var download = await contratosService.DescargarAsync(id);
    if (download is null)
    {
        return Results.NotFound();
    }

    return Results.Stream(download.FileStream, download.ContentType ?? "application/pdf", download.FileDownloadName);
}).RequireAuthorization("ContratosDownload");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
