using System;
using FluentValidation;
using CentroTerapia.Application.DTOs.Cita;

namespace CentroTerapia.Application.Validators
{
    public class CreateCitaDtoValidator : AbstractValidator<CreateCitaDto>
    {
        public CreateCitaDtoValidator()
        {
            RuleFor(a => a.PacienteId)
                .GreaterThan(0).WithMessage("Paciente ID must be greater than 0");

            RuleFor(a => a.TerapeutaId)
                .GreaterThan(0).When(x => x.TerapeutaId.HasValue).WithMessage("TerapeutaId must be greater than 0 when provided");

            RuleFor(a => a.DuracionMinutos)
                .GreaterThan(0).When(x => x.DuracionMinutos.HasValue).WithMessage("DuracionMinutos must be greater than 0 when provided");

            RuleFor(a => a.Fecha)
                .NotEmpty().WithMessage("Cita date is required")
                .GreaterThan(DateTime.Now).WithMessage("Cita date must be in the future");

            RuleFor(a => a.Motivo)
                .MaximumLength(200).WithMessage("Motivo cannot exceed 200 characters");
        }
    }
}

