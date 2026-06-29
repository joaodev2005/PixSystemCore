using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PixSystemCore.Domain.Exceptions;

namespace PixSystemCore.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = GetTitle(exception),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions = new Dictionary<string, object>
            {
                { "traceId", traceId },
                { "timestamp", DateTime.UtcNow }
            }
        };

        _logger.LogError(exception, "Erro processando requisição. TraceId: {TraceId}", traceId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status.Value;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        InsufficientBalanceException => StatusCodes.Status422UnprocessableEntity,
        AccountNotFoundException => StatusCodes.Status404NotFound,
        PixKeyNotFoundException => StatusCodes.Status404NotFound,
        DomainException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        DomainException => "Erro de negócio",
        _ => "Erro interno do servidor"
    };
}
