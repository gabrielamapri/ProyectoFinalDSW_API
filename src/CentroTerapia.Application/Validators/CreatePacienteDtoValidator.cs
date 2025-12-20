using FluentValidation;
using CentroTerapia.Application.DTOs.Paciente;

namespace CentroTerapia.Application.Validators
{
    public class CreatePacienteDtoValidator : AbstractValidator<CreatePacienteDto>
    {
        public CreatePacienteDtoValidator()
        {
            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Nombres es requerido")
                .MaximumLength(100).WithMessage("Nombres no puede exceder 100 caracteres");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Apellidos es requerido")
                .MaximumLength(100).WithMessage("Apellidos no puede exceder 100 caracteres");

            RuleFor(x => x.FechaNacimiento)
                .NotEmpty().WithMessage("Fecha de nacimiento es requerida")
                .LessThan(DateTime.Now).WithMessage("Fecha de nacimiento debe ser en el pasado");

            RuleFor(x => x.FamiliaId)
                .GreaterThan(0).WithMessage("FamiliaId debe ser mayor que 0");

            RuleFor(x => x.Sexo)
                .MaximumLength(20).WithMessage("Sexo no puede exceder 20 caracteres");

            RuleFor(x => x.NombreContactoEmergencia)
                .MaximumLength(100).WithMessage("Nombre de contacto no puede exceder 100 caracteres");

            RuleFor(x => x.NumeroContactoEmergencia)
                .MaximumLength(30).WithMessage("Número de contacto no puede exceder 30 caracteres");
        }
    }
}
