
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = CreateProblemDetails(context, exception);

            LogException(exception, problemDetails);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        }


        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            return exception switch
            {
                NotFoundException ex => new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc7807.html#section-6.1",
                    Title = "Recurso no encontrado",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },

                DuplicateEntityException ex => new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc7807.html#section-6.1",
                    Title = "Entidad duplicada",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },
                BusinessRuleException ex => new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc7807.html#section-6.1",
                    Title = "Regla de negocio violada",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },
                DomainException ex => new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc7807.html#section-6.1",
                    Title = "Error de dominio",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },

                _ => new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc7807.html#section-6.1",
                    Title = "Error interno del servidor",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "Ha Ocurrido un error inesperado. Por favor, intente mas tarde",
                    Instance = context.Request.Path
                }
            };
        }

        private void LogException(Exception exception, ProblemDetails problemDetails)
        {
            if (problemDetails.Status >= 500)
            {
                _logger.LogError(exception, "Error interno {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Error controlado: {Title} -  {Message}", problemDetails.Title, problemDetails.Detail);
            }

        }

    }
}
