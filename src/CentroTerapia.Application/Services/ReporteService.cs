using CentroTerapia.Application.DTOs.Reportes;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly ICitaService _citaService;
        private readonly IPacienteService _pacienteService;
        private readonly IFamiliaService _familiaService;
        private readonly INotaSesionService _notaSesionService;

        public ReporteService(
            ICitaService citaService,
            IPacienteService pacienteService,
            IFamiliaService familiaService,
            INotaSesionService notaSesionService)
        {
            _citaService = citaService;
            _pacienteService = pacienteService;
            _familiaService = familiaService;
            _notaSesionService = notaSesionService;
        }

        // 1. HISTORIAL CLÍNICO DEL PACIENTE
        public async Task<HistorialPacienteDto> GetHistorialPacienteAsync(int pacienteId, int page = 1, int pageSize = 10)
        {
            try
            {
                var paciente = await _pacienteService.GetByIdAsync(pacienteId);
                if (paciente == null)
                    throw new NotFoundException($"Paciente con ID {pacienteId} no encontrado", pacienteId);

                var citas = await _citaService.GetByPacienteIdAsync(pacienteId);
                var citasOrdenadas = citas.OrderByDescending(c => c.Fecha).ToList();
                var totalCitas = citasOrdenadas.Count;
                var citasPaginadas = citasOrdenadas.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Notas recientes
                var notasRecientes = new List<NotaHistorialDto>(); // Simplificado por ahora

                var especialidad = citasOrdenadas.FirstOrDefault()?.EspecialidadNombre ?? "N/A";
                var terapeutaNombre = citasOrdenadas.FirstOrDefault()?.TerapeutaNombre ?? "N/A";

                return new HistorialPacienteDto
                {
                    PacienteId = paciente.Id,
                    PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
                    Edad = paciente.AgeInYears,
                    Especialidad = especialidad,
                    TerapeutaNombre = terapeutaNombre,
                    TotalCitas = totalCitas,
                    CitasCompletadas = citasOrdenadas.Count(c => c.Estado == "Completed"),
                    CitasCanceladas = citasOrdenadas.Count(c => c.Estado == "Cancelled"),
                    CitasProgramadas = citasOrdenadas.Count(c => c.Estado == "Scheduled"),
                    Citas = citasPaginadas.Select(c => new CitaHistorialDto
                    {
                        CitaId = c.Id,
                        Fecha = c.Fecha,
                        Estado = c.Estado,
                        Motivo = c.Motivo ?? "N/A",
                        TerapeutaNombre = c.TerapeutaNombre,
                        DuracionMinutos = c.DuracionMinutos
                    }).ToList(),
                    NotasRecientes = notasRecientes,
                    Pagination = new PaginationDto
                    {
                        Page = page,
                        PageSize = pageSize,
                        Total = totalCitas
                    }
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial del paciente: {ex.Message}");
            }
        }

        // 2. CONTROL DE ASISTENCIA
        public async Task<ControlAsistenciaDto> GetControlAsistenciaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var todasLasCitas = await _citaService.GetAllAsync();
                var citas = todasLasCitas
                    .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                    .ToList();

                var totalProgramadas = citas.Count;
                var totalAsistencias = citas.Count(c => c.Estado == "Completed");
                var totalCancelaciones = citas.Count(c => c.Estado == "Cancelled");
                var totalInasistencias = citas.Count(c => c.Estado == "Scheduled");
                var porcentaje = totalProgramadas > 0 ? (totalAsistencias / (decimal)totalProgramadas) * 100 : 0;

                var pacientesCitas = citas.GroupBy(c => c.PacienteId).ToList();
                var pacientes = new List<AsistenciaPacienteDto>();

                foreach (var pg in pacientesCitas)
                {
                    var citasDelPaciente = pg.ToList();
                    var paciente = await _pacienteService.GetByIdAsync(pg.Key);
                    if (paciente == null) continue;

                    var asistencias = citasDelPaciente.Count(c => c.Estado == "Completed");
                    var cancelaciones = citasDelPaciente.Count(c => c.Estado == "Cancelled");
                    var inasistencias = citasDelPaciente.Count(c => c.Estado == "Scheduled");
                    var porcentajePaciente = citasDelPaciente.Count > 0 ? (asistencias / (decimal)citasDelPaciente.Count) * 100 : 0;

                    pacientes.Add(new AsistenciaPacienteDto
                    {
                        PacienteId = paciente.Id,
                        PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
                        Edad = paciente.AgeInYears,
                        Especialidad = citasDelPaciente.FirstOrDefault()?.EspecialidadNombre ?? "N/A",
                        TotalCitas = citasDelPaciente.Count,
                        Asistencias = asistencias,
                        Cancelaciones = cancelaciones,
                        Inasistencias = inasistencias,
                        PorcentajeAsistencia = (decimal)porcentajePaciente,
                        Detalles = citasDelPaciente.Select(c => new DetalleAsistenciaDto
                        {
                            CitaId = c.Id,
                            Fecha = c.Fecha,
                            Estado = c.Estado,
                            Motivo = c.Motivo
                        }).ToList()
                    });
                }

                return new ControlAsistenciaDto
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalCitasProgramadas = totalProgramadas,
                    TotalAsistencias = totalAsistencias,
                    TotalCancelaciones = totalCancelaciones,
                    TotalInasistencias = totalInasistencias,
                    PorcentajeAsistencia = (decimal)porcentaje,
                    Pacientes = pacientes.OrderByDescending(p => p.PorcentajeAsistencia).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener control de asistencia: {ex.Message}");
            }
        }

        // 3. ESTADO DE CUENTA FAMILIA
        public async Task<EstadoCuentaFamiliaDto> GetEstadoCuentaFamiliaAsync(int familiaId)
        {
            try
            {
                var familia = await _familiaService.GetByIdAsync(familiaId);
                if (familia == null)
                    throw new NotFoundException($"Familia con ID {familiaId} no encontrada", familiaId);

                var estadoCuenta = new EstadoCuentaFamiliaDto
                {
                    FamiliaId = familia.Id,
                    ResponsablePrincipalNombre = familia.ResponsablePrincipalNombre,
                    ResponsablePrincipalEmail = familia.ResponsablePrincipalEmail ?? "N/A",
                    ResponsablePrincipalTelefono = familia.ResponsablePrincipalTelefono ?? "N/A",
                    Pacientes = new List<EstadoCuentaPacienteDto>()
                };

                // Obtener todos los pacientes de la familia
                var todasLasCitas = await _citaService.GetAllAsync();

                foreach (var paciente in familia.Pacientes)
                {
                    var citasDelPaciente = todasLasCitas.Where(c => c.PacienteId == paciente.Id).ToList();
                    var citasCompletadas = citasDelPaciente.Where(c => c.Estado == "Completed").ToList();
                    var citasProgramadas = citasDelPaciente.Where(c => c.Estado == "Scheduled").ToList();

                    var precioPorSesion = citasCompletadas.FirstOrDefault()?.Precio ?? 0;
                    var totalSesiones = citasCompletadas.Count * precioPorSesion;

                    var estadoPaciente = new EstadoCuentaPacienteDto
                    {
                        PacienteId = paciente.Id,
                        PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
                        Edad = paciente.AgeInYears,
                        Especialidad = citasCompletadas.FirstOrDefault()?.EspecialidadNombre ?? "N/A",
                        SesionesRealizadas = citasCompletadas.Count,
                        PrecioPorSesion = precioPorSesion,
                        TotalSesiones = totalSesiones,
                        MontoAbonado = 0,
                        SaldoDeuda = totalSesiones,
                        SesionesProximas = citasProgramadas.Count,
                        CostoSesionesProximas = citasProgramadas.Count * precioPorSesion,
                        DeudaTotal = totalSesiones + (citasProgramadas.Count * precioPorSesion)
                    };

                    estadoCuenta.Pacientes.Add(estadoPaciente);
                    estadoCuenta.TotalDeuda += estadoPaciente.DeudaTotal;
                }

                return estadoCuenta;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estado de cuenta: {ex.Message}");
            }
        }

        // 4. REPORTE DE PROGRESO DEL NIÑO
        public async Task<ReporteProgresoNinoDto> GetReporteProgresoNinoAsync(int pacienteId, int mes, int año)
        {
            try
            {
                var paciente = await _pacienteService.GetByIdAsync(pacienteId);
                if (paciente == null)
                    throw new NotFoundException($"Paciente con ID {pacienteId} no encontrado", pacienteId);

                var todasLasCitas = await _citaService.GetAllAsync();
                var citas = todasLasCitas
                    .Where(c => c.PacienteId == pacienteId &&
                               c.Fecha.Year == año &&
                               c.Fecha.Month == mes)
                    .ToList();

                var citasCompletadas = citas.Count(c => c.Estado == "Completed");
                var citasInasistidas = citas.Count(c => c.Estado != "Completed");
                var porcentaje = citas.Count > 0 ? (citasCompletadas / (decimal)citas.Count) * 100 : 0;

                var areas = new List<AreaTrabajoDto>
                {
                    new AreaTrabajoDto { Area = "Evaluación en progreso", Estado = "En evaluación" }
                };

                var terapeutaNombre = citas.FirstOrDefault()?.TerapeutaNombre ?? "Sin asignar";
                var especialidad = citas.FirstOrDefault()?.EspecialidadNombre ?? "N/A";

                return new ReporteProgresoNinoDto
                {
                    PacienteId = paciente.Id,
                    PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
                    Edad = paciente.AgeInYears,
                    Especialidad = especialidad,
                    TerapeutaNombre = terapeutaNombre,
                    MesReporte = new DateTime(año, mes, 1),
                    TotalSesiones = citas.Count,
                    SesionesAsistidas = citasCompletadas,
                    SesionesInasistidas = citasInasistidas,
                    PorcentajeAsistencia = (decimal)porcentaje,
                    AreasDetrabajo = areas,
                    TareasParaCasa = new List<string> { "Practicar según indicaciones del terapeuta" },
                    ProximosObjetivos = new List<string> { "Continuar con evaluación y establecer plan de intervención" },
                    Recomendaciones = "Continuar asistiendo regularmente a las sesiones programadas"
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener reporte de progreso: {ex.Message}");
            }
        }

        // 5. CITAS PRÓXIMAS
        public async Task<CitasProximasDto> GetCitasProximasAsync(int diasAnticipacion = 7)
        {
            try
            {
                var ahora = DateTime.Now;
                var fechaLimite = ahora.AddDays(diasAnticipacion);

                var todasLasCitas = await _citaService.GetAllAsync();
                var citas = todasLasCitas
                    .Where(c => c.Fecha >= ahora &&
                               c.Fecha <= fechaLimite &&
                               c.Estado == "Scheduled")
                    .OrderBy(c => c.Fecha)
                    .ToList();

                var citasDto = citas.Select(c => new CitaProximaDetalleDto
                {
                    CitaId = c.Id,
                    Fecha = c.Fecha,
                    HoraInicio = c.Fecha.Hour,
                    MinutoInicio = c.Fecha.Minute,
                    PacienteNombre = c.PacienteNombre,
                    PacienteEdad = 0, // Simplificado
                    Especialidad = c.EspecialidadNombre,
                    TerapeutaNombre = c.TerapeutaNombre,
                    ResponsableNombre = "N/A", // Simplificado
                    ResponsableTelefono = "N/A",
                    ResponsableEmail = "N/A",
                    Estado = c.Estado,
                    Confirmada = false
                }).ToList();

                return new CitasProximasDto
                {
                    FechaConsulta = ahora,
                    DiasDesdHoy = diasAnticipacion,
                    Citas = citasDto
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas próximas: {ex.Message}");
            }
        }
    }
}
