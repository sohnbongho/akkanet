using BenchMark.Performance;
using BenchMark.Test.Json;
using BenchMark.Test.ParentDipose;
using BenchMark.Test.SnowFlake;
using BenchmarkDotNet.Running;

namespace BenchMark;

internal class Program
{
    static void Main(string[] args)
    {


        //var config = ManualConfig.Create(DefaultConfig.Instance)
        //    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
        //    .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));

        //BenchmarkRunner.Run<IEnumeratorTest>();
        //BenchmarkRunner.Run<CryptographyTest>();
        //BenchmarkRunner.Run<ProtoBufferToArrayTest>();
        //BenchmarkRunner.Run<MemoryStreamTest>();
        BenchmarkRunner.Run<SessionSendTest>();

        //BenchmarkRunner.Run<DateTimeTest>();
        //BenchmarkRunner.Run<RedisConnectPoolTest>();

        //var eCSSystemTest = new ECSSystemTest();
        //eCSSystemTest.Test();

        //var system = new DecoratorPattern();
        //system.Test();

        //var system = new AttributeTest();
        //system.Test();

        //var system = new ParentDiposeTest();
        //system.Test();

        //var system = new SnowFlakeTest();
        //system.Test();
        //system.WorldIndexTest();

        //var jsonTest = new JsonSerial();
        //jsonTest.Test();
    }
}
