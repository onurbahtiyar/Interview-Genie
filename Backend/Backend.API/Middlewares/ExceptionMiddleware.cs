using System.Net;
using System.Text.Json;

namespace Backend.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        string message;

        switch (exception)
        {
            case UnauthorizedAccessException _:
                status = HttpStatusCode.Unauthorized;
                message = "Yetkisiz erişim.";
                break;
            case ArgumentException _:
                status = HttpStatusCode.BadRequest;
                message = "Geçersiz istek.";
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                message = "Sunucu hatası.";
                break;
        }

        var response = new { message };
        var payload = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(payload);
    }
}
