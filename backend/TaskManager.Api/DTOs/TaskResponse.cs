namespace TaskManager.Api.Dtos;

public record TaskResponse(
    int Id,
    string Titulo,
    string Descricao,
    string Status,
    DateTime DataCriacao);
