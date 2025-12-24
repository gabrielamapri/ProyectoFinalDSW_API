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

        // 2. CITAS PRÓXIMAS
        public async Task<CitasProximasDto> GetCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null)
        {
            try
            {
                // Validaciones
                if (fechaDesde.Date < DateTime.Today)
                    throw new ArgumentException("La fecha desde no puede ser anterior a hoy");

                if (fechaHasta.Date < fechaDesde.Date)
                    throw new ArgumentException("La fecha hasta no puede ser anterior a la fecha desde");

                if ((fechaHasta.Date - fechaDesde.Date).Days > 365)
                    throw new ArgumentException("El rango de fechas no puede superar 1 año");

                var todasLasCitas = await _citaService.GetAllAsync();
                var citas = todasLasCitas
                    .Where(c => c.Fecha.Date >= fechaDesde.Date &&
                               c.Fecha.Date <= fechaHasta.Date &&
                               c.Estado == "Scheduled")
                    .ToList();

                if (especialidadId.HasValue)
                {
                    citas = citas.Where(c => c.EspecialidadId == especialidadId.Value).ToList();
                }

                // Aplicar filtros opcionales (solo terapeuta y tipo sesión tienen IDs en CitaDto)
                if (terapeutaId.HasValue)
                {
                    citas = citas.Where(c => c.TerapeutaId == terapeutaId.Value).ToList();
                }

                if (tipoSesionId.HasValue)
                {
                    citas = citas.Where(c => c.TipoSesionId == tipoSesionId.Value).ToList();
                }

                citas = citas.OrderBy(c => c.Fecha).ToList();

                // Construir DTOs con datos completos
                var citasDto = new List<CitaProximaDetalleDto>();

                foreach (var cita in citas)
                {
                    // Obtener datos del paciente y familia
                    var paciente = await _pacienteService.GetByIdAsync(cita.PacienteId);
                    
                    string responsableNombre = "N/A";
                    string responsableTelefono = "N/A";

                    if (paciente != null && paciente.FamiliaId.HasValue)
                    {
                        var familia = await _familiaService.GetByIdAsync(paciente.FamiliaId.Value);
                        if (familia != null)
                        {
                            responsableNombre = $"{familia.ResponsablePrincipalNombre ?? ""} {familia.ResponsablePrincipalApellido ?? ""}".Trim();
                            if (string.IsNullOrWhiteSpace(responsableNombre))
                                responsableNombre = "N/A";
                            
                            responsableTelefono = familia.ResponsablePrincipalTelefono ?? familia.TelefonoContacto ?? "N/A";
                        }
                    }

                    citasDto.Add(new CitaProximaDetalleDto
                    {
                        CitaId = cita.Id,
                        Fecha = cita.Fecha,
                        HoraInicio = cita.Fecha.Hour,
                        MinutoInicio = cita.Fecha.Minute,
                        PacienteNombre = cita.PacienteNombre ?? "N/A",
                        Especialidad = cita.EspecialidadNombre ?? "N/A",
                        TipoSesion = cita.TipoSesionNombre ?? "N/A",
                        TerapeutaNombre = cita.TerapeutaNombre ?? "N/A",
                        ResponsableNombre = responsableNombre,
                        ResponsableTelefono = responsableTelefono
                    });
                }

                return new CitasProximasDto
                {
                    FechaConsulta = DateTime.Now,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    TotalCitas = citasDto.Count,
                    Citas = citasDto
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas próximas: {ex.Message}");
            }
        }

        // 3. HISTORIAL DE CITAS (solo pasado)
        public async Task<HistorialCitasDto> GetHistorialCitasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int? especialidadId = null,
            int? terapeutaId = null,
            int? tipoSesionId = null,
            string? estado = null,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var ayer = DateTime.Today.AddDays(-1);
                if (fechaHasta.Date > ayer)
                    throw new ArgumentException("La fecha hasta debe ser como máximo el día anterior a hoy");

                if (fechaDesde.Date > fechaHasta.Date)
                    throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta");

                if ((fechaHasta.Date - fechaDesde.Date).Days > 365)
                    throw new ArgumentException("El rango de fechas no puede superar 1 año");

                var todasLasCitas = await _citaService.GetAllAsync();
                var citas = todasLasCitas
                    .Where(c => c.Fecha.Date >= fechaDesde.Date && c.Fecha.Date <= fechaHasta.Date)
                    .ToList();

                if (especialidadId.HasValue)
                    citas = citas.Where(c => c.EspecialidadId == especialidadId.Value).ToList();

                if (terapeutaId.HasValue)
                    citas = citas.Where(c => c.TerapeutaId == terapeutaId.Value).ToList();

                if (tipoSesionId.HasValue)
                    citas = citas.Where(c => c.TipoSesionId == tipoSesionId.Value).ToList();

                if (!string.IsNullOrWhiteSpace(estado))
                    citas = citas.Where(c => string.Equals(c.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

                var citasOrdenadas = citas.OrderBy(c => c.Fecha).ToList();
                var total = citasOrdenadas.Count;
                var pageSafe = Math.Max(page, 1);
                var sizeSafe = Math.Max(pageSize, 1);
                var citasPaginadas = citasOrdenadas.Skip((pageSafe - 1) * sizeSafe).Take(sizeSafe).ToList();

                var detalles = citasPaginadas.Select(c => new CitaDetalleDto
                {
                    CitaId = c.Id,
                    Fecha = c.Fecha,
                    HoraInicio = c.Fecha.Hour,
                    MinutoInicio = c.Fecha.Minute,
                    PacienteNombre = c.PacienteNombre ?? "N/A",
                    Especialidad = c.EspecialidadNombre ?? "N/A",
                    TipoSesion = c.TipoSesionNombre ?? "N/A",
                    TerapeutaNombre = c.TerapeutaNombre ?? "N/A",
                    Estado = c.Estado ?? "",
                    Motivo = c.Motivo ?? ""
                }).ToList();

                return new HistorialCitasDto
                {
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    EspecialidadId = especialidadId,
                    TerapeutaId = terapeutaId,
                    TipoSesionId = tipoSesionId,
                    EstadoFiltro = estado,
                    TotalCitas = total,
                    Completadas = citasOrdenadas.Count(c => c.Estado == "Completed"),
                    Canceladas = citasOrdenadas.Count(c => c.Estado == "Cancelled"),
                    Programadas = citasOrdenadas.Count(c => c.Estado == "Scheduled"),
                    Citas = detalles,
                    Pagination = new PaginationDto
                    {
                        Page = pageSafe,
                        PageSize = sizeSafe,
                        Total = total
                    }
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial de citas: {ex.Message}");
            }
        }

        public async Task<byte[]> ExportHistorialCitasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int? especialidadId = null,
            int? terapeutaId = null,
            int? tipoSesionId = null,
            string? estado = null)
        {
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var reporte = await GetHistorialCitasAsync(fechaDesde, fechaHasta, especialidadId, terapeutaId, tipoSesionId, estado, 1, 1000);

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
                                col.Item().Text("Historial de Citas").FontSize(14).Bold().AlignCenter();
                                col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).AlignCenter();
                                col.Item().PaddingVertical(8);
                            });
                        });

                        page.Content().Element(content =>
                        {
                            content.Column(col =>
                            {
                                col.Item().Text($"Rango: {reporte.FechaDesde:dd/MM/yyyy} - {reporte.FechaHasta:dd/MM/yyyy}").FontSize(11).Bold();
                                col.Item().Text($"Filtros → Especialidad: {(reporte.EspecialidadId?.ToString() ?? "Todas")}, Terapeuta: {(reporte.TerapeutaId?.ToString() ?? "Todos")}, Tipo Sesión: {(reporte.TipoSesionId?.ToString() ?? "Todas")}, Estado: {(string.IsNullOrWhiteSpace(reporte.EstadoFiltro) ? "Todos" : reporte.EstadoFiltro)}").FontSize(9);
                                col.Item().Text($"Total: {reporte.TotalCitas} | Completadas: {reporte.Completadas} | Programadas: {reporte.Programadas} | Canceladas: {reporte.Canceladas}").FontSize(9);
                                col.Item().PaddingBottom(8);

                                col.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.1f);   // Fecha
                                        columns.RelativeColumn(0.9f);   // Hora
                                        columns.RelativeColumn(2.0f);   // Paciente
                                        columns.RelativeColumn(1.6f);   // Especialidad
                                        columns.RelativeColumn(1.6f);   // Tipo Sesión
                                        columns.RelativeColumn(1.8f);   // Terapeuta
                                        columns.RelativeColumn(1.2f);   // Estado
                                        columns.RelativeColumn(2.0f);   // Motivo
                                    });

                                    t.Header(header =>
                                    {
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Fecha").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Hora").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Paciente").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Especialidad").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Tipo Sesión").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Terapeuta").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Estado").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Motivo").FontSize(9).Bold();
                                    });

                                    foreach (var c in reporte.Citas)
                                    {
                                        var fechaStr = c.Fecha != DateTime.MinValue ? c.Fecha.ToString("dd/MM/yyyy") : "—";
                                        var horaStr = $"{c.HoraInicio:00}:{c.MinutoInicio:00}";

                                        t.Cell().Padding(3).Text(fechaStr).FontSize(8);
                                        t.Cell().Padding(3).Text(horaStr).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.PacienteNombre) ? "N/A" : c.PacienteNombre).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.Especialidad) ? "N/A" : c.Especialidad).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.TipoSesion) ? "N/A" : c.TipoSesion).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.TerapeutaNombre) ? "N/A" : c.TerapeutaNombre).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.Estado) ? "N/A" : c.Estado).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(c.Motivo) ? "" : c.Motivo).FontSize(8);
                                    }
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
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar historial de citas: {ex.Message}");
            }
        }

        public async Task<byte[]> ExportCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null)
        {
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var reporte = await GetCitasProximasAsync(fechaDesde, fechaHasta, especialidadId, terapeutaId, tipoSesionId);

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
                                col.Item().Text("Reporte de Citas Próximas").FontSize(14).Bold().AlignCenter();
                                col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).AlignCenter();
                                col.Item().PaddingVertical(8);
                            });
                        });

                        page.Content().Element(content =>
                        {
                            content.Column(col =>
                            {
                                col.Item().Text($"Rango: {reporte.FechaDesde:dd/MM/yyyy} - {reporte.FechaHasta:dd/MM/yyyy}").FontSize(11).Bold();
                                col.Item().Text($"Total de citas: {reporte.TotalCitas}").FontSize(10);
                                col.Item().Text($"Filtros → Especialidad: {(especialidadId.HasValue ? especialidadId.Value.ToString() : "Todas")}, Terapeuta: {(terapeutaId.HasValue ? terapeutaId.Value.ToString() : "Todos")}, Tipo Sesión: {(tipoSesionId.HasValue ? tipoSesionId.Value.ToString() : "Todas")}").FontSize(9);
                                col.Item().PaddingBottom(8);

                                col.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.1f);   // Fecha
                                        columns.RelativeColumn(0.9f);   // Hora
                                        columns.RelativeColumn(2.0f);   // Paciente
                                        columns.RelativeColumn(1.6f);   // Especialidad
                                        columns.RelativeColumn(1.6f);   // Tipo Sesión
                                        columns.RelativeColumn(1.8f);   // Terapeuta
                                        columns.RelativeColumn(2.0f);   // Responsable
                                        columns.RelativeColumn(1.5f);   // Teléfono
                                    });

                                    t.Header(header =>
                                    {
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Fecha").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Hora").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Paciente").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Especialidad").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Tipo Sesión").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Terapeuta").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Responsable").FontSize(9).Bold();
                                        header.Cell().Background("#E8E8E8").Padding(5).Text("Teléfono").FontSize(9).Bold();
                                    });

                                    foreach (var cita in reporte.Citas)
                                    {
                                        var fechaStr = cita.Fecha != DateTime.MinValue ? cita.Fecha.ToString("dd/MM/yyyy") : "—";
                                        var horaStr = $"{cita.HoraInicio:00}:{cita.MinutoInicio:00}";

                                        t.Cell().Padding(3).Text(fechaStr).FontSize(8);
                                        t.Cell().Padding(3).Text(horaStr).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.PacienteNombre) ? "N/A" : cita.PacienteNombre).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.Especialidad) ? "N/A" : cita.Especialidad).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.TipoSesion) ? "N/A" : cita.TipoSesion).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.TerapeutaNombre) ? "N/A" : cita.TerapeutaNombre).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.ResponsableNombre) ? "N/A" : cita.ResponsableNombre).FontSize(8);
                                        t.Cell().Padding(3).Text(string.IsNullOrWhiteSpace(cita.ResponsableTelefono) ? "N/A" : cita.ResponsableTelefono).FontSize(8);
                                    }
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
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar citas próximas: {ex.Message}");
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
