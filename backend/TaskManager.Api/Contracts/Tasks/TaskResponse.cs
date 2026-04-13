namespace TaskManager.Api.Contracts.Tasks;

public record TaskResponse(
    int Id,
    string Titulo,
    string Descricao,
    string Status,
    DateTime DataCriacao);
