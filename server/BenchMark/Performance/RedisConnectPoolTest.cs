using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Library.Connector;
using Library.Memory.ProtoBuffer;
using Messages;
using MongoDB.Driver.Core.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMark.Performance;

[MemoryDiagnoser]
public class RedisConnectPoolTest
{    
    [Benchmark]
    public ConnectionMultiplexer PoolTest()
    {
        var connectionStr = "192.168.10.45:6379,allowAdmin=true,password=...,syncTimeout=10000,connectTimeout=10000";
        RedisConnectionPool.Instance.Init(connectionStr, 5);
        var connection = RedisConnectionPool.Instance.GetConnection();

        return connection;

    }

    [Benchmark]
    public ConnectionMultiplexer Normal()
    {
        var connectionStr = "192.168.10.45:6379,allowAdmin=true,password=...,syncTimeout=10000,connectTimeout=10000";
        RedisConnectionPool.Instance.Init(connectionStr, 5);
        var connection = RedisConnectionPool.Instance.GetConnectionWithNormal();
        return connection;
    }
}
