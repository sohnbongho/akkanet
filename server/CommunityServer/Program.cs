using log4net;
using CommunityServer.Helper;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using log4net.Config;

namespace CommunityServer;
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
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB로 설정

                if (usedHttps)
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

        // Akka HOCON 정보 읽어오기            
        if (false == Load())
        {
            throw new Exception("failed to config load");
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
        return true;
    }                
}