using RazorLight;

namespace LinkUpPro.Infrastructure.Shared.Messaging;

public class RazorRenderer : IRazorRenderer
{
    private readonly IRazorLightEngine _engine;

    public RazorRenderer()
    {
        _engine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<T>(string templatePath, T model)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"La plantilla de correo no se encontro en: {templatePath}");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        return await _engine.CompileRenderStringAsync(templatePath, templateContent, model);
    }
}
