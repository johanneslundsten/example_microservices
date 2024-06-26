namespace ApplicationUtils;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

public class LoggingErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingErrorHandler> _logger;

    public LoggingErrorHandler(RequestDelegate next, ILogger<LoggingErrorHandler> logger)
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
            _logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = "Internal Server Error",
            details = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
