using System.Net;
using System.Text.Json;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;

namespace Stylo.Backend.Stylo.API.Middleware
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
                _logger.LogError(ex, "Unhandled exception occurred while processing request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string errorCode;
            string message;

            if (exception is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                errorCode = appEx.ErrorCode;
                message = appEx.Message;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                errorCode = "INTERNAL_SERVER_ERROR";
                message = "An unexpected error occurred on the server.";
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = new ErrorResponseDto
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = JsonSerializer.Serialize(errorResponse, jsonOptions);
            return context.Response.WriteAsync(result);
        }
    }
}
