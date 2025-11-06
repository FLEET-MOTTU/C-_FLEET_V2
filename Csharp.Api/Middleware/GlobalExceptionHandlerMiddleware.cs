using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Csharp.Api.Exceptions;

namespace Csharp.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next,
                                                ILogger<GlobalExceptionHandlerMiddleware> logger,
                                                IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            object? errorDetails = null;
            string errorMessage = "Ocorreu um erro inesperado.";

            if (_env.IsDevelopment())
            {
                errorDetails = new
                {
                    type = exception.GetType().Name,
                    error = exception.Message,
                    stackTrace = exception.ToString()
                };
            }

            switch (exception)
            {
                case MotoNotFoundException:
                case BeaconNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorMessage = exception.Message;
                    break;

                case PlacaJaExisteException:
                case TagJaExisteException:
                case BeaconJaExisteException:
                case ConcorrenciaException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorMessage = exception.Message;
                    break;

                case EntradaInvalidaException:
                case BusinessRuleException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorMessage = exception.Message;
                    break;

                case DbUpdateConcurrencyException dbConcurrencyEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorMessage = "Os dados foram modificados por outra transação. Por favor, tente novamente.";
                    _logger.LogWarning(dbConcurrencyEx, "Conflito de concorrência detectado.");
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorMessage = exception.Message;
                    break;
            }

            var payload = new
            {
                statusCode = response.StatusCode,
                message = errorMessage,
                details = _env.IsDevelopment() ? errorDetails : null
            };

            return response.WriteAsync(JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ));
        }
    }
}
