namespace BenchMark.Test.SnowFlake;

public class SnowFlakeTest
{
    public void Test()
    {
        SnowFlakeGerneratorTest.Instance.SetWorkerId(1);

        var uidHashSet = new HashSet<ulong>();

        //{
        //    var dateTime = new DateTime(2025, 3, 11, 0, 0, 0, DateTimeKind.Utc);

        //    var uid = SnowFlakeGerneratorTest.Instance.NextId(dateTime.Ticks);
        //    uidHashSet.Add(uid);
        //    Console.WriteLine($"dateTime:{dateTime} uid:{uid}");
        //}

        //{
        //    var dateTime = new DateTime(2035, 3, 11, 0, 0, 0, DateTimeKind.Utc);

        //    var uid = SnowFlakeGerneratorTest.Instance.NextId(dateTime.Ticks);
        //    uidHashSet.Add(uid);
        //    Console.WriteLine($"dateTime:{dateTime} uid:{uid}");
        //}

        var startDateTime = new DateTime(2035, 3, 11, 0, 0, 0, DateTimeKind.Utc);
        ulong lastUid = 0;
        for (int i = 0; i < 800; ++i)
        {
            startDateTime += TimeSpan.FromMilliseconds(1);
            var uid = (ulong)SnowFlakeGerneratorTest.Instance.NextId(startDateTime.Ticks);
            uidHashSet.Add(uid);
            if (lastUid >= uid)
                Console.WriteLine($"Error dateTime:{startDateTime} lastUid:{lastUid} uid:{uid:X8}");

            lastUid = (ulong)uid;
            Console.WriteLine($"dateTime:{startDateTime} uid:{uid:X8}");
        }
    }

    public void WorldIndexTest()
    {
        var dateTime = new DateTime(2025, 3, 11, 0, 0, 0, DateTimeKind.Utc);
        {
            var serverId = 2;
            SnowFlakeGerneratorTest.Instance.SetWorkerId(serverId);
            var uid = (ulong)SnowFlakeGerneratorTest.Instance.NextId(dateTime.Ticks);
            Console.WriteLine($"dateTime:{dateTime} uid:{uid:X8}");
        }
        {
            var serverId = 2;
            SnowFlakeGerneratorTest.Instance.SetWorkerId(serverId);
            var uid = (ulong)SnowFlakeGerneratorTest.Instance.NextId(dateTime.Ticks);
            Console.WriteLine($"dateTime:{dateTime} uid:{uid:X8}");
        }
        {
            var serverId = 1;
            SnowFlakeGerneratorTest.Instance.SetWorkerId(serverId);
            var uid = (ulong)SnowFlakeGerneratorTest.Instance.NextId(dateTime.Ticks);
            Console.WriteLine($"dateTime:{dateTime} uid:{uid:X8}");
        }


    }
}
