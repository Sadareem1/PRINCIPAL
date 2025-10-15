using Microsoft.Extensions.Options;

namespace SGI_INVENTARIO.Services;

public class AlertWorker : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertWorker> _logger;
    private readonly JobSchedulerOptions _options;
    private Timer? _timer;

    public AlertWorker(IServiceScopeFactory scopeFactory, IOptions<JobSchedulerOptions> options, ILogger<AlertWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var nextRun = GetNextRun();
        var delay = nextRun - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        _timer = new Timer(async _ => await EjecutarAsync(), null, delay, TimeSpan.FromHours(24));
        _ = EjecutarAsync();
        return Task.CompletedTask;
    }

    private async Task EjecutarAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var alertasService = scope.ServiceProvider.GetRequiredService<IAlertasService>();
            await alertasService.GenerarAlertasVencimientosAsync();
            _logger.LogInformation("AlertWorker ejecutado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando AlertWorker");
        }
    }

    private DateTime GetNextRun()
    {
        var utcNow = DateTime.UtcNow;
        var target = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, _options.DailyCheckHour, 0, 0, DateTimeKind.Utc);
        if (utcNow > target)
        {
            target = target.AddDays(1);
        }
        return target;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public class JobSchedulerOptions
{
    public int DailyCheckHour { get; set; } = 8;
}
