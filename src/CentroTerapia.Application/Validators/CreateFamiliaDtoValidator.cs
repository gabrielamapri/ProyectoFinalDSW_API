using FluentValidation;
using CentroTerapia.Application.DTOs.Familia;

namespace CentroTerapia.Application.Validators
{
    public class CreateFamiliaDtoValidator : AbstractValidator<CreateFamiliaDto>
    {
        public CreateFamiliaDtoValidator()
        {
            RuleFor(x => x.Responsable1Nombre)
                .MaximumLength(50).WithMessage("Responsable1Nombre no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable1Apellido)
                .MaximumLength(50).WithMessage("Responsable1Apellido no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable1DNI)
                .MaximumLength(20).WithMessage("Responsable1DNI no puede exceder 20 caracteres");

            RuleFor(x => x.Responsable1Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Responsable1Email)).WithMessage("Responsable1Email debe ser un email válido");

            RuleFor(x => x.Responsable2Nombre)
                .MaximumLength(50).WithMessage("Responsable2Nombre no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable2Apellido)
                .MaximumLength(50).WithMessage("Responsable2Apellido no puede exceder 50 caracteres");

            RuleFor(x => x.Responsable2DNI)
                .MaximumLength(20).WithMessage("Responsable2DNI no puede exceder 20 caracteres");

            RuleFor(x => x.Responsable2Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Responsable2Email)).WithMessage("Responsable2Email debe ser un email válido");

            RuleFor(x => x.TelefonoContacto)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.TelefonoContacto)).WithMessage("TelefonoContacto no puede exceder 20 caracteres");
        }
    }
}
