using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LobbyServer.Helper;
using System.Reflection;
using System.Net;
using log4net.Config;
using Library.Helper;
using Microsoft.Extensions.Logging; // 로그 설정을 위한 네임스페이스 추가

namespace LobbyServer;

internal class Program
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var port = ConfigHelper.Instance.SwaggerPort;
        var usedHttps = ConfigHelper.Instance.UsedHttps;

        return Host.CreateDefaultBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<AkkaHostedService>();
        })
        .ConfigureLogging(logging =>
        {            
            logging.SetMinimumLevel(LogLevel.Warning); // Warning 이상만 출력
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB로 설정

                if(usedHttps)
                {
                    // HTTPS 엔드포인트 추가
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.UseHttps("certificate.pfx", "1111");
                    });
                }
                else
                {
                    // HTTP 엔드포인트에서 리스닝
                    options.Listen(IPAddress.Any, port);
                }

                
            })
            .UseUrls($"https://0.0.0.0:{port}"); // HTTPS URL로 변경
        });
    }

    static async Task Main(string[] args)
    {
        var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

        // 정보 읽어오기                        
        if (false == Load())
        {
            throw new Exception("failed to load");
        }

        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static bool Load()
    {
        if (false == ConfigHelper.Instance.Load())
        {
            throw new Exception("failed to config load");                
        }
        if (false == ItemDBLoaderHelper.Instance.Load())
        {
            throw new Exception("failed to iteminfo load");
        }
        if (false == ExpDBLoaderHelper.Instance.Load())
        {
            throw new Exception("failed to exp db load");
        }
        if (false == ConstDbHelper.Instance.Load())
        {
            throw new Exception("failed to const db load");
        }
        
        if (false == DailyStampDataRepo.Instance.Load())
        {
            throw new Exception("failed to DailyStamp db load");
        }

        return true;
    }       
    
}