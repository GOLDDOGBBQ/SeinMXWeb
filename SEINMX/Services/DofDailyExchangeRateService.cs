
using System.Text.Json;
using SEINMX.Clases.Generales;
using SEINMX.Clases.Tools;
using CargoBajaLib.Service;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;

namespace SEINMX.Services;

public class DofDailyExchangeRateService : CronBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;


    //  private readonly ClApiRequest _apiRequest;
    private readonly ILogger<DofDailyExchangeRateService> _logger;
    private readonly ExchangeRateService _exchangeRateService;
    // private readonly EmailServiceFactory _emailServiceFactory;

    public DofDailyExchangeRateService(
        CronConfigServiceProvider cronConfigServiceProvider,
        ILogger<DofDailyExchangeRateService> logger,
        //   IGlobal<ClApiRequest> apiRequestFactory,
        ExchangeRateService exchangeRateService,
        //      IGlobal<EmailServiceFactory> emailServiceFactory,
        IServiceScopeFactory scopeFactory
    ) : base(cronConfigServiceProvider)
    {
        //  _apiRequest = apiRequestFactory.Value;
        _logger = logger;
        _exchangeRateService = exchangeRateService;
        //   _emailServiceFactory = emailServiceFactory.Value;
        _scopeFactory = scopeFactory;
    }

    private AppClassContext CreateClassContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppClassContext>();
    }

    protected override async Task Exec(CancellationToken cancellationToken)
    {
        var fecha = DateTime.Now;

        try
        {
            await Retry(
                new[] { TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30) },
                async () => { await ActualizaTc(cancellationToken, fecha); },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await HandleError(cancellationToken, fecha, ex);
            throw;
        }
    }

    private async Task ActualizaTc(CancellationToken cancellationToken, DateTime fecha)
    {
        var tipoCambio = await _exchangeRateService.ExecuteRequestAsync(fecha, cancellationToken);

        using var db = CreateClassContext();

        var result = db
            .SpGenericResults
            .FromSqlInterpolated($@"
                EXEC [CFG].[GPApi_TipoCambioNva]
                     @Json = {JsonSerializer.Serialize(new
                     {
                         TipoCambio = tipoCambio,
                         Fecha = fecha
                     })},
                     @UserId = 'PROCESOS',
                     @ProgName = '{GetType().Name}'
            ")
            .AsEnumerable()
            .FirstOrDefault();


        // Si el SP no regresó nada (caso muy raro)
        if (result == null)
        {
            throw new ClApiResponseException( "No se recibió respuesta del servidor de la BD.");
        }

        // Si el SP reporta error
        if (result.IdError != 0)
        {
            throw new ClApiResponseException( result.MensajeError + " " + result.MensajeErrorDev);
        }
    }

    private async Task HandleError(CancellationToken cancellationToken, DateTime fecha, Exception ex)
    {
        /*try
        {
            var emailService = _emailServiceFactory.Create();
            await emailService.SendEmailAsync(
                recipients: await _apiRequest.ConsultaDestinatarios("NOTIFTC"),
                cc: null,
                bcc: null,
                subject: $"ERROR: NO SE PUDO CAPTURAR EL TIPO DE CAMBIO // {fecha:dd-MM-yyyy}",
                textBody:
                "El tipo de cambio de hoy no se capturó automáticamente debido a un error en el DOF. Introdúzcalo en el sistema manualmente." +
                "\n\n" +
                "Error: " + '\n' +
                ExceptionDiagnostics.PrettyPrintException(ex),
                htmlBody: null,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception)
        {*/
        _logger.LogError(ex, "NO SE PUDO CAPTURAR EL TIPO DE CAMBIO");
        //  }
    }
}
