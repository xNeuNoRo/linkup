namespace LinkUpPro.Infrastructure.Shared.Messaging;

public interface IRazorRenderer
{
    Task<string> RenderTemplateAsync<T>(string templatePath, T model);
}
