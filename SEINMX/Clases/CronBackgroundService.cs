

using System.Collections.Concurrent;
using SEINMX.Clases.Generales;
using CargoBajaLib;
using CargoBajaLib.Cron;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;
using SEINMX.Context.Database;


namespace SEINMX.Clases.Tools;

public abstract class CronBackgroundService : BackgroundService
{
    private readonly CronService _cronService;
    private readonly CargoBajaLib.ILogger _logger;

    protected CronBackgroundService(CronConfigServiceProvider cronConfigServiceProvider)
    {
        _logger = cronConfigServiceProvider.Logger;

        var serviceName = GetType().Name;
        var cronConfigService = cronConfigServiceProvider.Register(serviceName);
        cronConfigService.ExecutionEvent += () =>
        {
            Task.Run(async () => { await Exec(cronConfigServiceProvider.CancellationToken); });
        };

        _cronService = new CronService(
            serviceName,
            Exec,
            cronConfigService,
            _logger
        );
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return _cronService.ExecuteAsync(cancellationToken);
    }

    protected abstract Task Exec(CancellationToken cancellationToken);

    protected async Task<T> Retry<T>(
        TimeSpan[] retryDelays,
        Func<Task<T>> action,
        CancellationToken cancellationToken
    )
    {
        return await RetryableAction.Retry(
            _logger,
            retryDelays,
            action,
            cancellationToken
        );
    }

    protected async Task Retry(
        TimeSpan[] retryDelays,
        Func<Task> action,
        CancellationToken cancellationToken
    )
    {
        await RetryableAction.Retry(
            _logger,
            retryDelays,
            action,
            cancellationToken
        );
    }
}

public class CronConfigServiceProvider
{
    public CargoBajaLib.ILogger Logger { get; }
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, ClApiCronConfigService> _services = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public CronConfigServiceProvider(
        IServiceScopeFactory scopeFactory,
        ILogger<CronService> logger,
        IHostApplicationLifetime applicationLifetime
    )
    {
        _scopeFactory = scopeFactory;
        Logger =  logger.ToCargoBajaLibLogger();

        applicationLifetime.ApplicationStopping.Register(() => { _cancellationTokenSource.Cancel(); });
    }
    public ClApiCronConfigService Register(string name)
    {

        var service = new ClApiCronConfigService(_scopeFactory, name, true);
        var added = _services.TryAdd(name, service);

        if (!added)
        {
            throw new Exception($"A CronBackgroundService with name {name} already exists");
        }

        return service;
    }

    public ClApiCronConfigService Get(string name)
    {
        if (_services.TryGetValue(name, out var service)) return service;

        service = new ClApiCronConfigService(_scopeFactory,name,  false);
        service = _services.GetOrAdd(name, service);
        return service;
    }
}

public class ClApiCronConfigService : ICronConfigService
{
    private readonly string _serviceName;
    private readonly bool _available;
    private readonly IServiceScopeFactory _scopeFactory;

    public ClApiCronConfigService(IServiceScopeFactory scopeFactory, string serviceName,  bool available)
    {
        _scopeFactory = scopeFactory;
        _serviceName = serviceName;
        _available = available;
    }

    private AppDbContext CreateDb()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public event Action? ConfigUpdated;
    public event Action? ExecutionEvent;

    public async Task<CronServiceConfig> GetCronServiceConfig()
    {
        using var db = CreateDb();
        var tareaProgramada = await db.TareaProgramada.FirstOrDefaultAsync(x => x.Nombre == _serviceName);



        return new CronServiceConfig(
            tareaProgramada.ExpresionCron,
            tareaProgramada.UltimaEjecucion ?? tareaProgramada.FchActivacion,
            tareaProgramada.Activa,
            TimeZoneParser.ParseTimeZone(tareaProgramada.ZonaHoraria)
        );
    }

    public async Task UpdateLastExecutionTime(DateTimeOffset lastExecutionTime)
    {
        using var db = CreateDb();
        var tareaProgramada = await db.TareaProgramada.FirstOrDefaultAsync(x => x.Nombre == _serviceName);

        if(tareaProgramada == null) return;

        tareaProgramada.UltimaEjecucion = lastExecutionTime;
        tareaProgramada.ModificadoPor = _serviceName ;
        tareaProgramada.FchAct = DateTime.Now ;
        await db.SaveChangesAsync();
    }

    public async Task<int> UpdateConfig(
        string descripcion,
        string expresionCron,
        bool activa,
        string zonaHoraria,
        string progName,
        string usuario
    )
    {
        try
        {
            TimeZoneParser.ParseTimeZone(zonaHoraria);
        }
        catch (Exception e)
        {
            throw new ClApiResponseException("Zona horaria no válida", e.Message);
        }

        try
        {
            CronExpression.Parse(expresionCron);
        }
        catch (Exception e)
        {
            throw new ClApiResponseException("Error de formato", e.Message);
        }

        using var db = CreateDb();
        var tarea = await db.TareaProgramada.FirstOrDefaultAsync(x => x.Nombre == _serviceName);

        var utcNow = DateTimeOffset.UtcNow;
        string action;

        if (tarea != null)
        {
            // MATCHED → UPDATE
            tarea.Descripcion   = descripcion;
            tarea.ExpresionCron = expresionCron;
            tarea.Activa        = activa;
            tarea.ZonaHoraria   = zonaHoraria;
            tarea.ModificadoPor = progName;
            tarea.FchAct        = DateTime.Now;
            tarea.UsrAct        = usuario;

            // Lógica exacta del CASE
            if (!activa)
            {
                tarea.FchActivacion = null;
            }
            else if (activa && tarea.Activa)
            {
                // Se conserva la fecha
            }
            else
            {
                tarea.FchActivacion = utcNow;
            }

            action = "UPDATE";
        }
        else
        {
            // NOT MATCHED → INSERT
            tarea = new TareaProgramadum
            {
                Nombre = _serviceName,
                Descripcion = descripcion,
                ExpresionCron = expresionCron,
                Activa = activa,
                ZonaHoraria = zonaHoraria,
                FchActivacion = activa ? utcNow : null,
                CreadoPor = progName,
                FchReg = DateTime.Now,
                UsrReg = usuario
            };

            db.TareaProgramada.Add(tarea);
            action = "INSERT";
        }

        await db.SaveChangesAsync();



        var (idTareaProgramada, actionPerformed) =(tarea.IdTareaProgramada, action);

        if (actionPerformed == "UPDATE")
        {
            ConfigUpdated?.Invoke();
        }

        return idTareaProgramada;
    }

    public void Exec()
    {
        if (!_available)
        {
            throw new InvalidOperationException($"The CronBackgroundService {_serviceName} is not available");
        }

        ExecutionEvent?.Invoke();
    }

    private record ClCronConfigTareaProgramada(
        string ExpresionCron,
        DateTimeOffset? UltimaEjecucion,
        DateTimeOffset? FchActivacion,
        bool Activa,
        string ZonaHoraria
    );
}