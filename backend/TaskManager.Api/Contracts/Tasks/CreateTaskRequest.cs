namespace TaskManager.Api.Contracts.Tasks;

public sealed record CreateTaskRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
