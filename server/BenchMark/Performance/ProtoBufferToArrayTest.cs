using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Library.Memory.ProtoBuffer;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMark.Performance;

[MemoryDiagnoser]
public class ProtoBufferToArrayTest
{
    [Benchmark]
    public byte[] ToByteArray()
    {
        var messageWrapper = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        return messageWrapper.ToByteArray();

    }

    [Benchmark]
    public byte[] ToByteArrayWithPool()
    {
        var messageWrapper = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        return messageWrapper.ToByteArrayWithPool();

    }

    [Benchmark]
    public ArraySegment<byte> ToArraySegmentWithPool()
    {
        var messageWrapper = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        return messageWrapper.ToArraySegmentWithPool();
    }

    [Benchmark]
    public byte[] ToByteArraySafe()
    {
        var messageWrapper = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        return messageWrapper.ToByteArraySafe();
    }
    [Benchmark]
    public byte[] ToByteArrayWithBuffer()
    {
        var messageWrapper = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        return messageWrapper.ToByteArrayWithBuffer();
    }


}
