using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Diagnostics;

public class RequestLoggingFilter : IActionFilter
{
    private readonly Serilog.ILogger _logger;//注入serilog
    private Stopwatch _stopwatch;//统计程序耗时

    public RequestLoggingFilter(Serilog.ILogger logger)
    {
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch.Stop();
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;
        _logger
            .ForContext("RequestJson", request.QueryString)//请求字符串
            .ForContext("ResponseJson", JsonConvert.SerializeObject(context.Result))//响应数据json
            .Information("Request {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms",//message
            request.Method,
            request.Path,
            response.StatusCode,
            _stopwatch.Elapsed.TotalMilliseconds);
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}