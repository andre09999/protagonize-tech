using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Dtos;

public class UpdateTaskRequest
{
    [Required(ErrorMessage = "O titulo e obrigatorio.")]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descricao e obrigatoria.")]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O status e obrigatorio.")]
    public string Status { get; set; } = "Pendente";
}
