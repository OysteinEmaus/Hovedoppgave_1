using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hovedoppgave.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var statusCode = HttpStatusCode.InternalServerError; // 500 default
            var message = "An unexpected error occurred";

            // Egendefinert logikk for å håndtere spesifikke unntak
            if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized; // 401
                message = "Unauthorized access";
            }
            else if (exception is ArgumentException || exception is FormatException)
            {
                statusCode = HttpStatusCode.BadRequest; // 400
                message = exception.Message;
            }
            else if (exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = HttpStatusCode.NotFound; // 404
                message = exception.Message;
            }

            // Sette statuskode og innholdstype
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            // Lage ny feilmelding
            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                // Inkluder detaillert informasjon i debug-modus
#if DEBUG
                Detail = exception.ToString()
#endif
            };

            // Error response til JSON
            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Skriv responsen til HTTP-responsen
            await context.Response.WriteAsync(result);
        }
    }
}