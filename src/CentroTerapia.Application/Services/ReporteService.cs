using CentroTerapia.Application.DTOs.Reportes;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Exceptions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

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

                // Cargar familia del paciente
                string responsableNombre = "";
                string responsableDNI = "";
                string responsableTelefono = "";
                string responsableEmail = "";

                if (paciente.FamiliaId.HasValue)
                {
                    var familia = await _familiaService.GetByIdAsync(paciente.FamiliaId.Value);
                    if (familia != null)
                    {
                        responsableNombre = $"{familia.ResponsablePrincipalNombre ?? ""} {familia.ResponsablePrincipalApellido ?? ""}".Trim();
                        responsableDNI = familia.ResponsablePrincipalDNI ?? "";
                        responsableTelefono = familia.ResponsablePrincipalTelefono ?? familia.TelefonoContacto ?? "";
                        responsableEmail = familia.ResponsablePrincipalEmail ?? "";
                    }
                }

                var citas = (await _citaService.GetByPacienteIdAsync(pacienteId) ?? Enumerable.Empty<DTOs.Cita.CitaDto>()).ToList();
                var citasOrdenadas = citas.OrderByDescending(c => c.Fecha).ToList();
                var totalCitas = citasOrdenadas.Count;
                var citasPaginadas = citasOrdenadas.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Obtener notas de sesión para las citas paginadas
                var todasLasNotas = (await _notaSesionService.GetAllAsync() ?? Enumerable.Empty<DTOs.NotaSesion.NotaSesionDto>()).ToList();
                var citasConNotas = citasPaginadas.Select(c =>
                {
                    var nota = todasLasNotas.FirstOrDefault(n => n.CitaId == c.Id);
                    return new CitaHistorialDto
                    {
                        CitaId = c.Id,
                        Fecha = c.Fecha,
                        Estado = c.Estado,
                        TerapeutaNombre = c.TerapeutaNombre ?? "N/A",
                        Especialidad = c.EspecialidadNombre ?? "N/A",
                        TipoSesion = c.TipoSesionNombre ?? "N/A",
                        Notas = nota?.Notas ?? ""
                    };
                }).ToList();

                return new HistorialPacienteDto
                {
                    PacienteId = paciente.Id,
                    PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
                    PacienteDNI = paciente.DNI ?? "",
                    Edad = paciente.AgeInYears,
                    ResponsableNombre = responsableNombre,
                    ResponsableDNI = responsableDNI,
                    ResponsableTelefono = responsableTelefono,
                    ResponsableEmail = responsableEmail,
                    TotalCitas = totalCitas,
                    CitasCompletadas = citasOrdenadas.Count(c => c.Estado == "Completed"),
                    CitasCanceladas = citasOrdenadas.Count(c => c.Estado == "Cancelled"),
                    CitasProgramadas = citasOrdenadas.Count(c => c.Estado == "Scheduled"),
                    Citas = citasConNotas,
                    NotasRecientes = new List<NotaHistorialDto>(),
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

        // 5. EXPORTAR HISTORIAL A PDF
        public async Task<byte[]> ExportHistorialPacienteAsync(int pacienteId)
        {
            try
            {
                if (pacienteId <= 0)
                    throw new ArgumentException("El ID del paciente debe ser mayor a 0");

                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var historial = await GetHistorialPacienteAsync(pacienteId, 1, 1000); // Cargar todas sin paginar para PDF

                if (historial == null)
                    throw new NotFoundException($"No se pudo generar el historial para el paciente {pacienteId}", pacienteId);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);

                        page.Header().Element(header =>
                        {
                            header.Column(col =>
                            {
                                col.Item().Text("CENTRO DE TERAPIAS INFANTILES").FontSize(18).Bold().AlignCenter();
                                col.Item().Text("Historial Clínico del Paciente").FontSize(14).Bold().AlignCenter();
                                col.Item().PaddingVertical(10);
                            });
                        });

                        page.Content().Element(content =>
                        {
                            content.Column(col =>
                            {
                                // Datos del Paciente
                                col.Item().Column(infoPaciente =>
                                {
                                    infoPaciente.Item().Text($"Paciente: {historial.PacienteNombre}").FontSize(11).Bold();
                                    infoPaciente.Item().Text($"DNI: {historial.PacienteDNI}");
                                    infoPaciente.Item().Text($"Edad: {historial.Edad} años");
                                    infoPaciente.Item().PaddingBottom(5);
                                });

                                col.Item().Column(infoResponsable =>
                                {
                                    infoResponsable.Item().Text("Responsable:").FontSize(11).Bold();
                                    infoResponsable.Item().Text($"Nombre: {historial.ResponsableNombre}");
                                    infoResponsable.Item().Text($"DNI: {historial.ResponsableDNI}");
                                    infoResponsable.Item().Text($"Teléfono: {historial.ResponsableTelefono}");
                                    infoResponsable.Item().Text($"Email: {historial.ResponsableEmail}");
                                    infoResponsable.Item().PaddingBottom(10);
                                });

                                // Estadísticas
                                col.Item().Column(stats =>
                                {
                                    stats.Item().Text("Estadísticas de Citas").FontSize(11).Bold();
                                    stats.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(cell => cell.Border(1).Padding(5).Text($"Total: {historial.TotalCitas}").AlignCenter());
                                        row.RelativeItem().Element(cell => cell.Border(1).Padding(5).Text($"Completadas: {historial.CitasCompletadas}").AlignCenter());
                                        row.RelativeItem().Element(cell => cell.Border(1).Padding(5).Text($"Programadas: {historial.CitasProgramadas}").AlignCenter());
                                        row.RelativeItem().Element(cell => cell.Border(1).Padding(5).Text($"Canceladas: {historial.CitasCanceladas}").AlignCenter());
                                    });
                                    stats.Item().PaddingBottom(10);
                                });

                                // Tabla de Citas
                                col.Item().Column(tabla =>
                                {
                                    tabla.Item().Text("Historial de Citas").FontSize(11).Bold();
                                    tabla.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn(2);
                                        });

                                        // Encabezados
                                        t.Header(header =>
                                        {
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Fecha").FontSize(9).Bold();
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Estado").FontSize(9).Bold();
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Terapeuta").FontSize(9).Bold();
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Especialidad").FontSize(9).Bold();
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Tipo Sesión").FontSize(9).Bold();
                                            header.Cell().Background("#E8E8E8").Padding(5).Text("Notas").FontSize(9).Bold();
                                        });

                                        // Filas de citas
                                        foreach (var cita in historial.Citas ?? new List<CitaHistorialDto>())
                                        {
                                            var fechaFormato = cita.Fecha != DateTime.MinValue ? cita.Fecha.ToString("dd/MM/yyyy") : "—";
                                            t.Cell().Padding(3).Text(fechaFormato).FontSize(8);
                                            t.Cell().Padding(3).Text(TraducirEstado(cita.Estado ?? "")).FontSize(8);
                                            t.Cell().Padding(3).Text(cita.TerapeutaNombre ?? "—").FontSize(8);
                                            t.Cell().Padding(3).Text(cita.Especialidad ?? "—").FontSize(8);
                                            t.Cell().Padding(3).Text(cita.TipoSesion ?? "—").FontSize(8);
                                            t.Cell().Padding(3).Text(cita.Notas ?? "").FontSize(8);
                                        }
                                    });
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text(txt =>
                        {
                            txt.Span($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar historial a PDF: {ex.Message}");
            }
        }

        private string TraducirEstado(string estado)
        {
            return estado switch
            {
                "Scheduled" => "Programado",
                "Completed" => "Completado",
                "Cancelled" => "Cancelado",
                _ => estado
            };
        }
    }
}
