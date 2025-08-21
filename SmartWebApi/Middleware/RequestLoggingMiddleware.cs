using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace SmartWebApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers.Add("X-Correlation-ID", correlationId);

            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // Log request
            var requestBody = await ReadRequestBodyAsync(request);
            _logger.LogInformation(
                "HTTP {Method} {Path} started. Body: {Body}",
                request.Method,
                request.Path,
                requestBody
            );

            // Capture original response body stream
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                var responseBody = await ReadResponseBodyAsync(responseBodyStream);
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms. Body: {Body}",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseBody
                );
            }
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var requestBody = Encoding.UTF8.GetString(buffer);
        request.Body.Position = 0;
        return requestBody;
    }

    private async Task<string> ReadResponseBodyAsync(Stream responseBodyStream)
    {
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        return responseBody;
    }
}