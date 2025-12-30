using System;
using System.Net;
using System.Net.Sockets;
using SEINMX.Clases.Generales;
using SEINMX.Clases.Tools;
using SEINMX.Clases.Utilerias;
using CargoBajaLib.Facturacion.DescargaMasivaCfdi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SEINMX.Services;

namespace SEINMX.Config;

public static class BackgroundServiceConfig
{
    public static void ConfigureBackgroundServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddHostedService<DofDailyExchangeRateService>();


        /*builder.Services.AddHostedService(sp =>
        {
            var cronConfigServiceProvider = sp.GetRequiredService<CronConfigServiceProvider>();
            var emailServiceFactory = sp.GetRequiredService<IGlobal<EmailServiceFactory>>().Value;
            var apiRequest = sp.GetRequiredService<IGlobal<ClApiRequest>>().Value;
            var logger = sp.GetRequiredService<ILogger<RecordatorioVencimientoCertificadoSatService>>();
            return new RecordatorioVencimientoCertificadoSatService(
                cronConfigServiceProvider,
                emailServiceFactory,
                apiRequest,
                logger
            );
        });*/
    }
}