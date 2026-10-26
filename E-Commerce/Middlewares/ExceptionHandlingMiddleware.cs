using System.Net;
using System.Text.Json;

namespace E_Commerce.Middlewares;

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
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;

        switch (exception)
        {
            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound; 
                break;

            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest; 
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError; 
                break;
        }

        context.Response.StatusCode = statusCode;

        var responseModel = new
        {
            success = false,
            message = exception.Message,
            statusCode = statusCode
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(responseModel, options);

        return context.Response.WriteAsync(json);
    }
}