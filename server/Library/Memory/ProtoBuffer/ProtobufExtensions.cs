using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Memory.ProtoBuffer;

/********* 벤치 마크 결과 **************************/
//| Method                 | Mean      | Error    | StdDev   | Gen0   | Allocated |
//|----------------------- |----------:|---------:|---------:|-------:|----------:|
//| ToByteArray            |  62.10 ns | 1.235 ns | 1.321 ns | 0.0128 |     168 B |
//| ToByteArrayWithPool    |  77.88 ns | 1.104 ns | 1.033 ns | 0.0128 |     168 B |
//| ToArraySegmentWithPool |  68.73 ns | 1.394 ns | 1.432 ns | 0.0134 |     176 B |
//| ToByteArraySafe        | 194.22 ns | 3.594 ns | 3.186 ns | 0.3545 |    4632 B |
//| ToByteArrayWithBuffer  |  78.73 ns | 1.097 ns | 1.026 ns | 0.0196 |     256 B |

//Method: 테스트한 메서드의 이름입니다.
//Mean: 각 테스트의 평균 실행 시간입니다.단위는** 나노초(ns)** 로, 시간이 짧을수록 빠른 성능을 의미합니다.
//Error: 99.9% 신뢰 구간에서의 오차 범위입니다. 이 값은 실제 평균이 약간 다를 수 있는 범위를 의미합니다.
//StdDev (표준 편차): 각 실행 시간의 변동성입니다. 값이 클수록 실행 시간이 들쭉날쭉하다는 것을 의미하며, 작을수록 일정한 성능을 나타냅니다.
//Gen0/Gen1: 1000번의 연산마다 발생하는 GC(Garbage Collection) 횟수입니다. 
//    Gen0과 Gen1은 각각 첫 번째와 두 번째 세대의 메모리 청소를 나타냅니다.Gen0가 클수록 많은 객체가 메모리에서 해제되었음을 의미합니다.
//Allocated: 각 메서드가 한 번의 실행에서 할당한 메모리 크기입니다. 
//    단위는 바이트(B) 입니다. 메모리 할당이 많을수록 성능에 부정적인 영향을 줄 수 있습니다.

public static class ProtobufExtensions
{
    private const int MaxArrayLength = 1024 * 1024; // 1MB
    private static readonly ArrayPool<byte> s_arrayPool = ArrayPool<byte>.Shared;

    public static byte[] ToByteArrayWithPool(this IMessage message)
    {
        int size = message.CalculateSize();
        byte[]? rentedBuffer = null;

        try
        {
            // 버퍼 크기 결정 (여유 공간 포함)
            int extendSize = Math.Min(size / 10, 1024);// 10% 또는 최대 1KB 추가
            extendSize = extendSize == 0 ? 1 : extendSize;
            int bufferSize = size + extendSize; 

            // 풀에서 버퍼 대여
            rentedBuffer = s_arrayPool.Rent(bufferSize);

            // 메시지 직렬화
            using (var output = new CodedOutputStream(rentedBuffer))
            {
                message.WriteTo(output);
                int actualSize = (int)output.Position;

                // 실제 사용된 크기만큼만 복사
                byte[] result = new byte[actualSize];
                Buffer.BlockCopy(rentedBuffer, 0, result, 0, actualSize);
                return result;
            }
        }
        finally
        {
            // 대여한 버퍼 반환
            if (rentedBuffer != null)
            {
                s_arrayPool.Return(rentedBuffer);
            }
        }
    }

    public static ArraySegment<byte> ToArraySegmentWithPool(this IMessage message)
    {
        int size = message.CalculateSize();
        byte[]? rentedBuffer = null;

        try
        {
            int bufferSize = size + Math.Min(size / 10, 1024);
            rentedBuffer = s_arrayPool.Rent(bufferSize);

            using (var output = new CodedOutputStream(rentedBuffer))
            {
                message.WriteTo(output);
                int actualSize = (int)output.Position;

                // ArraySegment 반환 (복사 없음)
                return new ArraySegment<byte>(rentedBuffer, 0, actualSize);
            }
        }
        catch
        {
            if (rentedBuffer != null)
            {
                s_arrayPool.Return(rentedBuffer);
            }
            throw;
        }
    }
    public static byte[] ToByteArraySafe(this IMessage message)
    {
        //ProtoPreconditions.CheckNotNull(message, "message");
        //byte[] array = new byte[message.CalculateSize()];
        //CodedOutputStream codedOutputStream = new CodedOutputStream(array);
        //message.WriteTo(codedOutputStream);
        //codedOutputStream.CheckNoSpaceLeft();
        //return array;


        using (var memoryStream = new MemoryStream())
        {
            message.WriteTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
    public static byte[] ToByteArrayWithBuffer(this IMessage message)
    {
        ProtoPreconditions.CheckNotNull(message, "message");
        int size = message.CalculateSize();
        int extendSize = Math.Min(size / 10, 1024);// 10% 또는 최대 1KB 추가
        var minExtendSize = 8;
        extendSize = extendSize < minExtendSize ? minExtendSize : extendSize;

        byte[] buffer = new byte[size + extendSize]; // 추가 여유 공간
        using (var output = new CodedOutputStream(buffer))
        {
            message.WriteTo(output);
            return buffer.Take((int)output.Position).ToArray();
        }
    }    
}
