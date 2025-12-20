using FluentValidation;
using CentroTerapia.Application.DTOs.Terapeuta;

namespace CentroTerapia.Application.Validators
{
    public class CreateTerapeutaDtoValidator : AbstractValidator<CreateTerapeutaDto>
    {
        public CreateTerapeutaDtoValidator()
        {
            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Nombres es requerido")
                .MaximumLength(100);

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Apellidos es requerido")
                .MaximumLength(100);

            RuleFor(x => x.EspecialidadId)
                .GreaterThanOrEqualTo(0).When(x => x.EspecialidadId.HasValue);

            RuleFor(x => x.Presentacion)
                .MaximumLength(1000).WithMessage("Presentación no debe exceder 1000 caracteres");

            RuleFor(x => x.Telefono)
                .MaximumLength(30);

            RuleFor(x => x.Direccion)
                .MaximumLength(200);
        }
    }
}
