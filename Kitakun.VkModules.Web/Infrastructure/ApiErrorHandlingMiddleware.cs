namespace Kitakun.VkModules.Web.Infrastructure
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    public class ApiErrorHandlingMiddleware
    {
        private static WeakReference<JsonSerializer> _cachedSerializer = null;
        private readonly RequestDelegate _next;

        public ApiErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context, ILogger<ApiErrorHandlingMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
            {
                //response already written earlier in the pipeline
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var error = new
            {
                message = ex.Message
            };

            context.Response.ContentType = "application/json";

            JsonSerializer serializer = null;
            if (_cachedSerializer == null || !_cachedSerializer.TryGetTarget(out serializer))
            {
                serializer = new JsonSerializer();
                if (_cachedSerializer == null)
                {
                    _cachedSerializer = new WeakReference<JsonSerializer>(serializer);
                }
                else
                {
                    _cachedSerializer.SetTarget(serializer);
                }
            }

            using (var writer = new StreamWriter(context.Response.Body))
            {
                serializer.Serialize(writer, error);
                await writer.FlushAsync();
            }

            return;
        }
    }
}
