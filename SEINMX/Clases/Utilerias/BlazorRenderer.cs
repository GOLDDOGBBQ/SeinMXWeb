using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SEINMX.Clases.Utilerias;

public class BlazorRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public BlazorRenderer()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();
        _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    public async Task<string> RenderViewToStringUsingModelAsync<TComponent>(object model)
        where TComponent : IComponent
    {
        var dictionary = new Dictionary<string, object?>
        {
            { "Model", model }
        };

        var parameters = ParameterView.FromDictionary(dictionary);

        return await RenderViewToStringAsync<TComponent>(parameters);
    }

    public async Task<string> RenderViewToStringAsync<TComponent>(ParameterView parameterView)
        where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var rootComponent = await htmlRenderer.RenderComponentAsync<TComponent>(parameterView);
            return rootComponent.ToHtmlString();
        });

        return html;
    }
}