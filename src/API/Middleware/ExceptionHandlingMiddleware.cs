using System.Net;
using System.Text.Json;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Models;

namespace ShipSharp.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message, details) = exception switch
        {
            ValidationException valEx => (
                HttpStatusCode.UnprocessableEntity,
                "validation_error",
                "The request data is invalid. Please check your input.",
                valEx.Errors
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                "not_found",
                notFoundEx.Message,
                (List<ApiErrorDetail>?)null
            ),
            UnprocessableEntityException unprocEx => (
                HttpStatusCode.UnprocessableEntity,
                unprocEx.Code,
                unprocEx.Message,
                (List<ApiErrorDetail>?)null
            ),
            ForbiddenException forbEx => (
                HttpStatusCode.Forbidden,
                "forbidden",
                forbEx.Message,
                (List<ApiErrorDetail>?)null
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "unauthorized",
                "Authentication is required to access this resource.",
                (List<ApiErrorDetail>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "internal_server_error",
                "An unexpected error occurred. Please try again later.",
                (List<ApiErrorDetail>?)null
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(errorCode, message, details);
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var payload = JsonSerializer.Serialize(response, jsonOptions);

        return context.Response.WriteAsync(payload);
    }
}
