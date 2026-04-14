namespace TaskManager.Api.Infrastructure.Messaging;

public record TaskEventMessage(
    string Action,
    int TaskId,
    int UserId,
    string Titulo,
    string Status,
    DateTime OccurredAtUtc);
