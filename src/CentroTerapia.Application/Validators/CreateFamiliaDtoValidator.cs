using FluentValidation;
using CentroTerapia.Application.DTOs.Familia;

namespace CentroTerapia.Application.Validators
{
    public class CreateFamiliaDtoValidator : AbstractValidator<CreateFamiliaDto>
    {
        public CreateFamiliaDtoValidator()
        {
            RuleFor(x => x.ResponsablePrincipalNombre)
                .MaximumLength(50).WithMessage("ResponsablePrincipalNombre no puede exceder 50 caracteres");

            RuleFor(x => x.ResponsablePrincipalApellido)
                .MaximumLength(50).WithMessage("ResponsablePrincipalApellido no puede exceder 50 caracteres");

            RuleFor(x => x.ResponsablePrincipalDNI)
                .MaximumLength(20).WithMessage("ResponsablePrincipalDNI no puede exceder 20 caracteres");

            RuleFor(x => x.ResponsablePrincipalEmail)
                .NotEmpty().WithMessage("ResponsablePrincipalEmail es requerido")
                .EmailAddress().WithMessage("ResponsablePrincipalEmail debe ser un email válido");

            RuleFor(x => x.Responsable2Nombre)
                .MaximumLength(50).WithMessage("Responsable2Nombre no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable2Apellido)
                .MaximumLength(50).WithMessage("Responsable2Apellido no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable2DNI)
                .MaximumLength(20).WithMessage("Responsable2DNI no puede exceder 20 caracteres");

            // Responsable2Email removed — no validation needed

            RuleFor(x => x.TelefonoContacto)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.TelefonoContacto)).WithMessage("TelefonoContacto no puede exceder 20 caracteres");
        }
    }
}
