using Serilog;
using Serilog.Events;

namespace BQHRWebApi.Common
{
    public static class SeriLogExtend
    {
        public static void AddSerilLog(this ConfigureHostBuilder configureHostBuilder)
        {
            //输出模板
            string outputTemplate = "【{Level:u3}】{Timestamp:yyyy-MM-dd HH:mm:ss.fff}" +
                                    "{NewLine}#Msg#{Message:lj}" +
                                    "{NewLine}#Pro #{Properties:j}" +
                                    "{NewLine}#Exc#{Exception}" +
                                     new string('-', 50) + "{NewLine}";

            // 配置Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 排除Microsoft的日志
                .Enrich.FromLogContext() // 注册日志上下文
                .WriteTo.Logger(configure => configure // 输出到文件
                            .MinimumLevel.Debug()
                            .WriteTo.File( //每天生成一个新的日志，按天来存日志
                                $"logs\\{DateTime.Today.ToString("yyyy-MM-dd")}-log.txt", //定输出到滚动日志文件中，每天会创建一个新的日志，按天来存日志
                                retainedFileCountLimit: 90,
                                outputTemplate: outputTemplate
                            ))
                .CreateLogger();

            configureHostBuilder.UseSerilog(Log.Logger); // 注册serilog
        }
    }
}
