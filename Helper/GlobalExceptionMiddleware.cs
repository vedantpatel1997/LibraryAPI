using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.API.Helper
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
                // Pass the request to the next middleware/component
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle exception and generate response
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception details
            _logger.LogError($"An error occurred: {exception.Message}\n{exception.StackTrace}");

            // Set response status code
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                title = "Server error",
                statusCode = StatusCodes.Status500InternalServerError,
                message = "An unexpected error occurred. Please try again later.",
                details = exception.Message // Include exception message in dev-friendly response
            };


            // Return the error response
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
