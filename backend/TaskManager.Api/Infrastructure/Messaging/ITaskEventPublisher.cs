namespace TaskManager.Api.Infrastructure.Messaging;

public interface ITaskEventPublisher
{
    Task PublishAsync(TaskEventMessage message, CancellationToken cancellationToken = default);
}
