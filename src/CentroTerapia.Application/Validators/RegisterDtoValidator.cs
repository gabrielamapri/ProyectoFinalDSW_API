
using FluentValidation;
using CentroTerapia.Application.DTOs.Auth;

namespace CentroTerapia.Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        private readonly string[]  _validRoles = new[] { "Admin", "Terapeuta", "Padre" };

        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(50).WithMessage("Password cannot exceed 50 characters.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(80).WithMessage("Last name cannot exceed 80 characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => _validRoles.Contains(role))
                .WithMessage($"Role must be one of the following: {string.Join(", ", _validRoles)}");
        }
    }

    

}
