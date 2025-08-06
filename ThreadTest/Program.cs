using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using System.Reflection;

namespace ThreadTest;

public class Program
{
    static async Task Main(string[] args)
    {
        Config config = LoadAkkaHconConfig();
        using var system = ActorSystem.Create("AkkaSystem", config);

        var userActor = system.ActorOf(Props.Create(() => new UserManagerActor()), "usermanager");

        Console.WriteLine("Akka.NET ActorSystem 실행 중... 종료하려면 Ctrl+C 누르세요");

        // 시스템이 종료될 때까지 대기
        await system.WhenTerminated;
    }
    public static Config LoadAkkaHconConfig()
    {
        var fullPath = Assembly.GetExecutingAssembly().Location;
        var directoryPath = Path.GetDirectoryName(fullPath);

        string path = $@"{directoryPath}/AkkaHCON.conf"; // 수정해야 할 부분

        // 파일이 존재하는지 확인
        if (File.Exists(path) == false)
        {
            Config tmpConfig = new Config();
            return tmpConfig;
        }
        // 파일 내용 읽기
        string content = File.ReadAllText(path);

        var config = ConfigurationFactory.ParseString(content);
        return config;
    }
}
