
using System.Net;
using System.Text.Json;

namespace SystemQMS.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi xảy ra khi xử lý request: {Path}",
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            string message = "Đã xảy ra lỗi hệ thống.";

            if (_environment.IsDevelopment())
            {
                message = exception.Message;
            }

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = message,
                path = context.Request.Path.Value,
                details = _environment.IsDevelopment()
                    ? exception.ToString()
                    : null
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
