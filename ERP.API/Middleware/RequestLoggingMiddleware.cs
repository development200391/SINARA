using System.Text;

namespace ERP.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    private readonly RequestDelegate _next;
    private readonly bool _enabled;
    private readonly string _logDirectory;

    public RequestLoggingMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment env)
    {
        _next = next;
        _enabled = configuration.GetValue<bool>("RequestLogging:Enabled");
        var logDir = configuration.GetValue<string>("RequestLogging:LogDirectory") ?? "logs";
        _logDirectory = Path.IsPathRooted(logDir)
            ? logDir
            : Path.Combine(env.ContentRootPath, logDir);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_enabled)
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();
        using var requestReader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await requestReader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var originalBody = context.Response.Body;
        using var capturedBody = new MemoryStream();
        context.Response.Body = capturedBody;

        var startTime = DateTime.Now;
        try
        {
            await _next(context);
        }
        finally
        {
            var finishTime = DateTime.Now;

            capturedBody.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(capturedBody).ReadToEndAsync();

            capturedBody.Seek(0, SeekOrigin.Begin);
            await capturedBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            await WriteLogAsync(context, requestBody, responseBody, startTime, finishTime);
        }
    }

    private async Task WriteLogAsync(HttpContext context, string requestBody, string responseBody, DateTime startTime, DateTime finishTime)
    {
        var duration = finishTime - startTime;
        var fileName = $"requests-{startTime:yyyy-MM-dd-HH}.txt";
        var filePath = Path.Combine(_logDirectory, fileName);

        Directory.CreateDirectory(_logDirectory);

        var sb = new StringBuilder();
        sb.AppendLine($"=== {startTime:yyyy-MM-dd HH:mm:ss} ===");
        sb.AppendLine($"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
        sb.Append($"Start : {startTime:HH:mm:ss.fff} | ");
        sb.Append($"Finish : {finishTime:HH:mm:ss.fff} | ");
        sb.AppendLine($"Duration : {duration.TotalMilliseconds:F0}ms");
        sb.AppendLine("[REQUEST BODY]");
        sb.AppendLine(string.IsNullOrWhiteSpace(requestBody) ? "(empty)" : requestBody);
        sb.AppendLine($"[RESPONSE {context.Response.StatusCode}]");
        sb.AppendLine(string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody);
        sb.AppendLine("============================");
        sb.AppendLine();

        await _fileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
