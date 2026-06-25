using FluentValidation;

namespace AuthService.Application.UseCases.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100);
    }
}
