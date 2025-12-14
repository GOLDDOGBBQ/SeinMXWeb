using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PuppeteerSharp;


namespace SEINMX.Clases.Utilerias;

public class RazorViewToStringRenderer
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorViewToStringRenderer(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    private static readonly SemaphoreSlim BrowserFetcherSemaphore = new(1, 1);

    private async Task<IBrowser> GetOrCreateBrowserAsync()
    {
        await BrowserFetcherSemaphore.WaitAsync();

        try
        {
            var browserFetcher = new BrowserFetcher(SupportedBrowser.ChromeHeadlessShell);
            var download = await browserFetcher.DownloadAsync("129.0.6668.100");
            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = download.GetExecutablePath()
            });
        }
        finally
        {
            BrowserFetcherSemaphore.Release();
        }
    }

    public async Task<byte[]> RenderViewToPdfAsync<TModel>(string viewName, TModel model)
    {
        var html = await RenderViewToStringAsync(viewName, model);

        var pdfBytes = await ConvertHtmlToPdfAsync(html);
        return pdfBytes;
    }

    public async Task<string> RenderViewToPdfBase64Async<TModel>(string viewName, TModel model)
    {
        var bytes = await RenderViewToPdfAsync(viewName, model);
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var actionContext = GetActionContext();
        var view = FindView(viewName);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
        return output.ToString();
    }

    private IView FindView(string viewPath)
    {
        var getViewResult = _razorViewEngine.GetView(viewPath, viewPath, isMainPage: false);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        throw new InvalidOperationException($"Could not find view: {viewPath}");
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    public static string ConvertToAbsolutePath(string relativePath)
    {
        var currentDirectory = Environment.CurrentDirectory;

        if (relativePath.StartsWith("~/"))
        {
            relativePath = relativePath.Substring(2);
        }

        var absolutePath = Path.Combine(
            new[] { currentDirectory }.Concat(relativePath.Split('/')).ToArray()
        );

        return absolutePath;
    }

    public async Task<byte[]> ConvertHtmlFileToPdfAsync(string path)
    {
        var absolutePath = ConvertToAbsolutePath(path);
        var source = await File.ReadAllTextAsync(absolutePath);
        return await ConvertHtmlToPdfAsync(source);
    }

    public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent, string? footerTemplate = null)
    {
        var browser = await GetOrCreateBrowserAsync();

        var page = await browser.NewPageAsync();

        string workingDirectory = Environment.CurrentDirectory;

        var bootstrapCss = await System.IO.File.ReadAllTextAsync(
            Path.Combine(workingDirectory, "wwwroot/lib/bootstrap/dist/css/bootstrap.min.css")
        );

        htmlContent = htmlContent.Replace("</head>", $"<style>{bootstrapCss}</style> </head>");


        await page.SetContentAsync(htmlContent, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        var pdfOptions = new PdfOptions
        {
            PrintBackground = true,
            PreferCSSPageSize = true
        };

        if (footerTemplate != null)
        {
            pdfOptions.DisplayHeaderFooter = true;
            pdfOptions.HeaderTemplate = "<div></div>";
            pdfOptions.FooterTemplate = footerTemplate;
        }


        var pdfBytes = await page.PdfDataAsync(pdfOptions);

        await browser.CloseAsync();

        return pdfBytes;
    }
}

public record ClEmailModel(
    string Mensaje,
    bool IncludeFooter = false,
    ClEmailModelUnsubscribe? Unsubscribe = null,
    ClEmailModelMensajeConfidencial? MensajeConfidencial = null,
    bool MensajeEsHtml = false
);

public record ClEmailModelUnsubscribe(string Destinatario, IdiomaEmail Idioma);

public record ClEmailModelMensajeConfidencial(IdiomaEmail Idioma)
{
    public string Mensaje
    {
        get
        {
            return Idioma switch
            {
                IdiomaEmail.Esp =>
                    "Este mensaje contiene información confidencial, la cual es de carácter privilegiado, confidencial, y de acceso restringido conforme a la ley aplicable. Si el lector de este mensaje no es el destinatario previsto o agente responsable de la transmisión del mensaje al destinatario, se le notifica por este medio que cualquier difusión, distribución o copiado de este mensaje y su contenido esta prohibida terminantemente.\n\nEste correo electrónico fue enviado desde una dirección de correo electrónico exclusivamente de notificación que no admite mensajes.",
                IdiomaEmail.Eng =>
                    "This message contains confidential information which is privileged and exempt from disclosure under applicable law. If the reader of this message is not the intended recipient, or an employee or responsible for the transmission of the message to the recipient agent, you are hereby notified that any dissemination, distribution or copying of this message and its content is strictly prohibited.\n\nThis email was sent from an e-mail notification service and It does not support returned messages.",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
};

public enum IdiomaEmail
{
    Esp = 1,
    Eng = 2
}

public static class RazorViewToStringRendererExtensions
{
    public static async Task<string> RenderEmail(this RazorViewToStringRenderer renderer, ClEmailModel model)
    {
        return await renderer.RenderViewToStringAsync("~/Reportes/Utilerias/Email.cshtml", model);
    }
}