using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Library.Helper.Encrypt;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BenchMark.Performance;

[MemoryDiagnoser]
public class DateTimeTest
{
    [Benchmark()]
    public DateTime UTCNow() => DateTime.UtcNow;

    [Benchmark()]
    public DateTime Now() => DateTime.Now;

    [Benchmark()]
    public DateTime UTCNowToLocal() => DateTime.UtcNow.ToLocalTime();

    [Benchmark()]
    public DateTime UTCNowAddOffset() => DateTime.SpecifyKind(DateTime.UtcNow + TimeSpan.FromHours(9), DateTimeKind.Local);

    private int _lastTickCount = -1;
    private DateTime _lastDateTime;

    [Benchmark()]
    public DateTime CachedNow()
    {
        var tick = Environment.TickCount;
        if (Interlocked.CompareExchange(ref _lastTickCount, tick, tick) != tick)
        {
            _lastTickCount = tick;
            _lastDateTime = DateTime.SpecifyKind(DateTime.UtcNow + TimeSpan.FromHours(9), DateTimeKind.Local);
        }
        return _lastDateTime;
    }
}
