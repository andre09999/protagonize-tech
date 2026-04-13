namespace TaskManager.Api.Common.Models;

public record ApiErrorResponse(
    string Title,
    int Status,
    string Detail,
    string TraceId,
    IDictionary<string, string[]>? Errors = null);
