namespace BenchMark.Test.SnowFlake;

public class SnowFlakeGerneratorTest
{
    private static readonly Lazy<SnowFlakeGerneratorTest> lazy = new Lazy<SnowFlakeGerneratorTest>(() => new SnowFlakeGerneratorTest());

    public static SnowFlakeGerneratorTest Instance { get { return lazy.Value; } }

    private const int WorkerIdBits = 10;
    private const int SequenceBits = 12;

    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
    private const long MaxSequence = -1L ^ (-1L << SequenceBits);

    private const int WorkerIdShift = SequenceBits;
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits;

    private static readonly object Lock = new object();
    private long _lastTimestamp = -1L;
    private long _sequence = 0L;

    public readonly long Epoch;
    public long WorkerId { get; private set; } = 1;

    public SnowFlakeGerneratorTest()
    {
        var epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Epoch = epoch.Ticks / 10000;
    }

    public void SetWorkerId(long workerId)
    {
        if (workerId > MaxWorkerId || workerId < 0)
            throw new ArgumentException($"worker Id can't be greater than {MaxWorkerId} or less than 0");
        
        Console.WriteLine($"workerId:{workerId:X8} MaxWorkerId:{MaxWorkerId:X8} MaxSequence:{MaxSequence:X8} TimestampLeftShift:{TimestampLeftShift:X8} WorkerIdShift:{WorkerIdShift:X8}");

        WorkerId = workerId;
    }

    public ulong NextId(long ticks)
    {
        lock (Lock)
        {
            var timestamp = CurrentTime(ticks);
            if (timestamp < _lastTimestamp)
                throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} ticks");

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                    timestamp = UntilNextMillis(_lastTimestamp, ticks);
            }
            else
            {
                _sequence = 0L;
            }

            _lastTimestamp = timestamp;
            return (ulong)(((timestamp - Epoch) << TimestampLeftShift) |
                   (WorkerId << WorkerIdShift) |
                   _sequence);
        }
    }

    private long UntilNextMillis(long lastTimestamp, long ticks)
    {
        var timestamp = CurrentTime(ticks);
        while (timestamp <= lastTimestamp)
        {
            ticks += 10000;
            timestamp = CurrentTime(ticks);
        }
        return timestamp;
    }

    private long CurrentTime(long ticks)
    {
        //최대 지속 가능 시간(밀리초): (2 ^ 41 − 1)
        //최대 지속 가능 시간(년): 최대 지속 가능 시간(밀리초) / (1000 * 60 * 60 * 24 * 365.25)
        //이 계산을 통해 얻은 결과는 약 69.7년입니다.
        //따라서, DateTime.UtcNow.Ticks / 10000을 사용하면
        //Snowflake ID 생성기는 약 69.7년 동안 유니크한 ID를 생성

        return ticks / 10000;
    }
}
