using CommunityServer.Component.DataBase;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityServer.Service
{
    /// <summary>
    /// Controller와 Service(비지니스 파트)를 별도로 관리
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        //서비스 수명(Scope) 옵션
        //AddScoped: 요청마다 서비스의 새 인스턴스를 생성합니다.하나의 HTTP 요청 내에서는 동일한 인스턴스가 사용됩니다.
        //AddTransient: 서비스를 요청할 때마다 새로운 인스턴스를 생성합니다.이는 매우 짧은 수명의 서비스에 적합합니다.
        //AddSingleton: 애플리케이션 수명 동안 단 하나의 인스턴스만 생성하고 모든 요청에서 이 인스턴스를 공유합니다.

        /// <summary>
        /// Controller에서 사용하는 Service(비지니스 로직)들
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            return services;
        }
    }

}
