using log4net;
using System.Net;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;

namespace Backend.Transport
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private static readonly ILog log = LogManager.GetLogger(typeof(ExceptionHandlingMiddleware));
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                log.DebugFormat("Starting request for connection {0}",context.Connection.Id) ;
                
                await _next(context);
            }
            catch (Exception ex)
            {
                var message = $"Exception encountered for connection {context.Connection.Id}: {ex}";
                log.Error(message);
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An error occurred while processing your request.",
                Detailed = exception.Message
            };

            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
