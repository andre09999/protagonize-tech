using FluentAssertions;
using TaskManager.Api.Contracts.Auth;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Validators.Auth;
using TaskManager.Api.Validators.Tasks;

namespace TaskManager.Api.Tests;

public class ValidatorTests
{
    [Fact]
    public void CreateTaskRequestValidator_ShouldRejectInvalidStatus()
    {
        var validator = new CreateTaskRequestValidator();

        var result = validator.Validate(new CreateTaskRequest
        {
            Titulo = "Tarefa",
            Descricao = "Descricao valida",
            Status = "Em andamento"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateTaskRequest.Status));
    }

    [Fact]
    public void RegisterRequestValidator_ShouldRejectShortPassword()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequest
        {
            Username = "maria",
            Password = "123"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Password));
    }
}
