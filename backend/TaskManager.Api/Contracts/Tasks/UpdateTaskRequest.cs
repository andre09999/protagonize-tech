namespace TaskManager.Api.Contracts.Tasks;

public sealed record UpdateTaskRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
