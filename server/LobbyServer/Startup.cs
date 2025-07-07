using Akka.Actor;
using Akka.Configuration;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using LobbyServer.Helper;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics;
using LobbyServer.Service;
using Library.AkkaActors;
using Prometheus;

namespace LobbyServer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        Config config = AkkaHostedService.LoadAkkaHconConfig();

        services.AddControllers();
        services.AddSingleton<ActorSystem>(provider => ActorSystem.Create(ActorPaths.System, config));
        services.AddControllerServices(); // Controller에서 사용하는 서비스들을 등록한다.

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "LobbyServer API",
                Description = "Request LobbyServer",
                //TermsOfService = new Uri("https://example.com/terms"),
                //Contact = new OpenApiContact
                //{
                //    Name = "Example Contact",
                //    Url = new Uri("https://example.com/contact")
                //},
                //License = new OpenApiLicense
                //{
                //    Name = "Example License",
                //    Url = new Uri("https://example.com/license")
                //}
            });

            // Set the comments path for the Swagger JSON and UI.
            // XML 주석 파일 경로 설정
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var usedHttps = ConfigHelper.Instance.UsedHttps;
        var enableSwaggerUi = ConfigHelper.Instance.EnableSwaggerUi;

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    // Log the exception using log4net
                    var logger = LogManager.GetLogger(typeof(Startup));
                    logger.Error(exception?.Message ?? string.Empty, exception);

                    // 비동기 작업이 없는 경우에도 Task 반환
                    await Task.CompletedTask;
                });
            });

            if (usedHttps)
            {
                app.UseHsts(); // HSTS 설정 추가
            }
        }

        if (usedHttps)
        {
            app.UseHttpsRedirection(); // HTTPS 리디렉션 설정 추가
        }

        app.UseRouting();
        app.UseHttpMetrics(); // Prometheus - 기본 HTTP 메트릭 제공 (요청 수, 요청 지연 시간 등)

        app.UseSwagger();
        if (enableSwaggerUi)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                c.RoutePrefix = string.Empty;
            });
        }
            
        app.UseEndpoints(endpoints => { 
            endpoints.MapControllers();
            endpoints.MapMetrics(); // /metrics 엔드포인트를 Prometheus에 노출
        });
    }
}
