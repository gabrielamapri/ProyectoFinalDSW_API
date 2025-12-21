

using System.ComponentModel;
using AutoMapper;
using CentroTerapia.Application.DTOs.Cita;
// Veterinary remnants removed; mappings consolidated under Familia/Responsable
using CentroTerapia.Application.DTOs.Paciente;
using CentroTerapia.Application.DTOs.Terapeuta;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Responsable, Paciente, Terapeuta mappings
            CreateMap<Paciente, PacienteDto>()
                .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Nombres))
                .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Apellidos))
                .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))
                .ForMember(dest => dest.AgeInYears, opt => opt.MapFrom(src => src.CalcularEdad()))
                .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.DNI ?? string.Empty))
                .ForMember(dest => dest.Sexo, opt => opt.MapFrom(src => src.Sexo))
                .ForMember(dest => dest.NombreContactoEmergencia, opt => opt.MapFrom(src => src.NombreContactoEmergencia ?? string.Empty))
                .ForMember(dest => dest.NumeroContactoEmergencia, opt => opt.MapFrom(src => src.NumeroContactoEmergencia ?? string.Empty))
                .ForMember(dest => dest.FamiliaId, opt => opt.MapFrom(src => src.FamiliaId))
                .ForMember(dest => dest.ResponsableNombre, opt => opt.MapFrom(src => src.Familia != null ? ((src.Familia.ResponsablePrincipalNombre ?? string.Empty) + " " + (src.Familia.ResponsablePrincipalApellido ?? string.Empty)) : string.Empty))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion));

            CreateMap<Terapeuta, TerapeutaDto>()
                .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Nombres))
                .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Apellidos))
                .ForMember(dest => dest.EspecialidadId, opt => opt.MapFrom(src => src.EspecialidadId))
                .ForMember(dest => dest.EspecialidadNombre, opt => opt.MapFrom(src => src.Especialidad != null ? src.Especialidad.Nombre : string.Empty))
                .ForMember(dest => dest.Presentacion, opt => opt.MapFrom(src => src.Presentacion ?? string.Empty))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono ?? string.Empty))
                .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Direccion ?? string.Empty))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activo));
            // Responsable, Terapeuta mappings

            CreateMap<Cita, CitaDto>()
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.Notas, opt => opt.MapFrom(src => src.Notas ?? string.Empty))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
                .ForMember(dest => dest.PacienteId, opt => opt.MapFrom(src => src.PacienteId))
                .ForMember(dest => dest.PacienteNombre, opt => opt.MapFrom(src => src.Paciente != null ? (src.Paciente.Nombres + " " + src.Paciente.Apellidos) : string.Empty))
                .ForMember(dest => dest.PuedeSerCancelada, opt => opt.MapFrom(src => src.CanBeCancelada()))
                .ForMember(dest => dest.TipoSesionId, opt => opt.MapFrom(src => src.TipoSesionId))
                .ForMember(dest => dest.TipoSesionNombre, opt => opt.MapFrom(src => src.TipoSesion != null ? src.TipoSesion.Nombre : string.Empty))
                .ForMember(dest => dest.TerapeutaId, opt => opt.MapFrom(src => src.TerapeutaId))
                .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.DuracionMinutos));

            CreateMap<CreatePacienteDto, Paciente>()
                .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Nombres))
                .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Apellidos))
                .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))
                .ForMember(dest => dest.FamiliaId, opt => opt.MapFrom(src => src.FamiliaId))
                .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.DNI))
                .ForMember(dest => dest.Sexo, opt => opt.MapFrom(src => src.Sexo))
                .ForMember(dest => dest.NombreContactoEmergencia, opt => opt.MapFrom(src => src.NombreContactoEmergencia))
                .ForMember(dest => dest.NumeroContactoEmergencia, opt => opt.MapFrom(src => src.NumeroContactoEmergencia));

            CreateMap<CreateTerapeutaDto, Terapeuta>()
                .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Nombres))
                .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Apellidos))
                .ForMember(dest => dest.EspecialidadId, opt => opt.MapFrom(src => src.EspecialidadId))
                .ForMember(dest => dest.Presentacion, opt => opt.MapFrom(src => src.Presentacion))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
                .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Direccion));

            CreateMap<UpdateTerapeutaDto, Terapeuta>();

            CreateMap<UpdatePacienteDto, Paciente>();

            CreateMap<UpdateCitaDto, Cita>()
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.Notas, opt => opt.MapFrom(src => src.Notas))
                .ForMember(dest => dest.TerapeutaId, opt => opt.MapFrom(src => src.TerapeutaId))
                .ForMember(dest => dest.TipoSesionId, opt => opt.MapFrom(src => src.TipoSesionId))
                .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.DuracionMinutos))
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

            CreateMap<CreateCitaDto, Cita>()
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))
                .ForMember(dest => dest.PacienteId, opt => opt.MapFrom(src => src.PacienteId))
                .ForMember(dest => dest.TerapeutaId, opt => opt.MapFrom(src => src.TerapeutaId))
                .ForMember(dest => dest.TipoSesionId, opt => opt.MapFrom(src => src.TipoSesionId))
                .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.DuracionMinutos))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Scheduled"))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.Now));

            // TipoSesion mappings
            CreateMap<CentroTerapia.Domain.Entities.TipoSesion, CentroTerapia.Application.DTOs.Terapia.TipoSesionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.DuracionMinutos))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion ?? string.Empty));

            CreateMap<CentroTerapia.Application.DTOs.Terapia.CreateTipoSesionDto, CentroTerapia.Domain.Entities.TipoSesion>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.DuracionMinutos))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion));

            CreateMap<CentroTerapia.Application.DTOs.Terapia.UpdateTipoSesionDto, CentroTerapia.Domain.Entities.TipoSesion>();

            // Familia mappings
            CreateMap<Familia, CentroTerapia.Application.DTOs.Familia.FamiliaDto>()
                .ForMember(dest => dest.Pacientes, opt => opt.MapFrom(src => src.Pacientes));

            CreateMap<CentroTerapia.Application.DTOs.Familia.CreateFamiliaDto, Familia>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.Now));

            // Franjas (Disponibilidad) mapping
            CreateMap<FranjaDisponibilidad, CentroTerapia.Application.DTOs.Franja.FranjaDisponibilidadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TerapeutaId, opt => opt.MapFrom(src => src.TerapeutaId))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.DiaSemana, opt => opt.MapFrom(src => src.DiaSemana))
                .ForMember(dest => dest.HoraInicio, opt => opt.MapFrom(src => src.HoraInicio))
                .ForMember(dest => dest.HoraFin, opt => opt.MapFrom(src => src.HoraFin))
                .ForMember(dest => dest.Recurrente, opt => opt.MapFrom(src => src.Recurrente));

        }
    }
}

