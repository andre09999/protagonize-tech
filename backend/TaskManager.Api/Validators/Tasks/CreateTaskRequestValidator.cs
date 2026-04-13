using FluentValidation;
using TaskManager.Api.Common.Models;
using TaskManager.Api.Contracts.Tasks;

namespace TaskManager.Api.Validators.Tasks;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(request => request.Titulo)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Descricao)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.Status)
            .Must(status => TaskStatuses.All.Contains(TaskStatuses.Normalize(status)))
            .WithMessage("Status deve ser Pendente ou Concluida.");
    }
}
