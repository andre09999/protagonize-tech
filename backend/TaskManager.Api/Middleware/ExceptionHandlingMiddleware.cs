using FluentValidation;
using TaskManager.Api.Common.Exceptions;
using TaskManager.Api.Common.Models;

namespace TaskManager.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var (statusCode, title, detail, errors, logAsWarning) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Erro de validacao",
                "Um ou mais campos estao invalidos.",
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()),
                true),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso nao encontrado",
                notFoundException.Message,
                null,
                true),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                "Conflito na solicitacao",
                conflictException.Message,
                null,
                true),
            UnauthorizedAccessException unauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Nao autorizado",
                unauthorizedAccessException.Message,
                null,
                true),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado ao processar a solicitacao.",
                null,
                false)
        };

        if (logAsWarning)
        {
            _logger.LogWarning(exception, "Falha tratada na requisicao {TraceId}", traceId);
        }
        else
        {
            _logger.LogError(exception, "Falha nao tratada na requisicao {TraceId}", traceId);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(title, statusCode, detail, traceId, errors);

        await context.Response.WriteAsJsonAsync(response);
    }
}
