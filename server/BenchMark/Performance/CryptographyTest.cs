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
public class CryptographyTest
{
    [Benchmark]
    public byte[] EncryptPacket()
    {
        byte[] arr = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            arr[i] = (byte)i;
        }

        var result = CryptographyHelper.EncryptPacket(arr);
        return result;
    }

    [Benchmark]
    public byte[] EncryptDataStream()
    {
        byte[] arr = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            arr[i] = (byte)i;
        }

        var result = CryptographyHelper.EncryptDataStream(arr);
        return result;
    }

    [Benchmark]
    public byte[] DecryptPacket()
    {
        byte[] arr = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            arr[i] = (byte)i;
        }

        var result = CryptographyHelper.DecryptPacket(arr, 1);
        return result;
    }

    [Benchmark]
    public byte[] DecryptDataStream()
    {
        byte[] arr = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            arr[i] = (byte)i;
        }

        var result = CryptographyHelper.DecryptDataStream(arr, 1);
        return result;
    }
}
