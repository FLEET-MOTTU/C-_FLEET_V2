using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Csharp.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

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
                _logger.LogError(ex, "Ocorreu uma exceção não tratada durante a execução da requisição: {Message}", ex.Message);
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
                errorDetails = new { type = exception.GetType().Name, error = exception.Message, stackTrace = exception.ToString() };
            }

            switch (exception)
            {
                case MotoNotFoundException:
                case BeaconNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    errorMessage = exception.Message;
                    break;
                case PlacaJaExisteException:
                case TagJaExisteException:
                case BeaconJaExisteException:
                case ConcorrenciaException:
                    response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    errorMessage = exception.Message;
                    break;
                case EntradaInvalidaException:
                case BusinessRuleException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    errorMessage = exception.Message;
                    break;
                case DbUpdateConcurrencyException dbConcurrencyEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    errorMessage = "Os dados foram modificados por outra transação. Por favor, tente novamente.";
                    _logger.LogWarning(dbConcurrencyEx, "Conflito de concorrência detectado.");
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                    errorMessage = exception.Message;
                    break;
            }

            var errorResponsePayload = new
            {
                statusCode = response.StatusCode,
                message = errorMessage,
                details = _env.IsDevelopment() ? errorDetails : null
            };
            
            return response.WriteAsync(JsonSerializer.Serialize(errorResponsePayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}