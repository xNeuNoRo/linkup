using System.Net;
using System.Text.Json;
using LinkUpPro.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace LinkUpPro.WebApp.Middlewares;

/// <summary>
/// Middleware global para interceptar excepciones de dominio y negocio,
/// traduciéndolas a respuestas amigables para el cliente (TempData + SweetAlert).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITempDataDictionaryFactory tempDataFactory
    )
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error en el flujo de la aplicación.");
            await HandleExceptionAsync(context, ex, tempDataFactory);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ITempDataDictionaryFactory tempDataFactory
    )
    {
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message =
            "Ocurrió un error al procesar la solicitud. Inténtelo nuevamente.";
        string errorCode = "System.InternalError";

        // Excepciones específicas (DEBEN ir ANTES de DomainException porque heredan de él)
        if (exception is DomainValidationException validationEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            // Concatenar los mensajes específicos de los errores de validación
            if (validationEx.ValidationErrors != null && validationEx.ValidationErrors.Count > 0)
            {
                message = string.Join(
                    " ",
                    validationEx.ValidationErrors.Select(e => e.Message)
                );
            }
            else
            {
                message = validationEx.Message;
            }
            errorCode = validationEx.Code;
        }
        else if (exception is GameRuleException gameEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = gameEx.Message;
            errorCode = gameEx.Code;
        }
        else if (exception is FriendshipRuleException friendEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = friendEx.Message;
            errorCode = friendEx.Code;
        }
        else if (exception is ConcurrencyException concurrencyEx)
        {
            statusCode = (int)HttpStatusCode.Conflict;
            message = concurrencyEx.Message;
            errorCode = concurrencyEx.Code;
        }
        else if (exception is DomainException domainEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = domainEx.Message;
            errorCode = domainEx.Code;
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            message = "No tiene permisos para realizar esta acción.";
            errorCode = "System.Unauthorized";
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            message = "El recurso solicitado no fue encontrado.";
            errorCode = "System.NotFound";
        }

        bool isAjax =
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new { Message = message, Code = errorCode };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        else
        {
            // Guardar en TempData para mostrar en la siguiente vista
            var tempData = tempDataFactory.GetTempData(context);
            tempData["ErrorMessage"] = message;
            tempData["ErrorCode"] = errorCode;
            tempData.Save();

            var referer = context.Request.Headers["Referer"].ToString();

            if (
                !string.IsNullOrEmpty(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                && string.Equals(refererUri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase)
                && !referer.Contains("/Home/Error")
                && !referer.Contains("/Auth/Login")
                && !referer.Contains("/Auth/Register")
            )
            {
                context.Response.Redirect(referer);
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

/// <summary>
/// Método de extensión para facilitar el registro en Program.cs.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(
        this IApplicationBuilder builder
    )
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
