using Akka.IO;
using Library.Helper.Encrypt;
using Library.Memory.ProtoBuffer;
using Library.messages;
using log4net;
using Messages;
using System.Buffers.Binary;
using System.Reflection;

namespace Library.AkkaActors.Socket.Handler;

public class SessionSendHanlderWithFixMemory : ISessionSendHanlder, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    //private SessionPacket _packet = new();
    // Thread별 고정 버퍼    
    private SessionPacket? _packet;

    public void Init()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_packet != null)
        {
            _packet.Dispose();
            _packet = null;
        }
    }
    public bool Tell_Old(SessionInfoHandler session, MessageWrapper message)
    {
        var messageName = message.PayloadCase.ToString();
        ushort packetLength = 0;

        ushort totalSizeBytesLength = 0;
        ushort messageSizeBytesLength = 0;
        ushort binaryLength = 0;

        try
        {
            var packetEncrypt = session.PacketEncrypt;
            //var requestBinary = ToByteArraySafe(message);
            var requestBinary = message.ToByteArrayWithBuffer();

            message.MessageSize = requestBinary.Length;

            ushort totalSize = sizeof(ushort); // messageSize만 넣고                
            ushort messageSize = (ushort)requestBinary.Length;

            byte[]? binary = null;

            if (packetEncrypt)
            {
                binary = CryptographyHelper.EncryptPacket(requestBinary);
                totalSize += (ushort)binary.Length;
            }
            else
            {
                binary = requestBinary;
                totalSize += (ushort)requestBinary.Length;
            }

            if (_packet == null)
                _packet = new SessionPacket();

            {
                // 메모리 풀에서 필요한 만큼의 메모리를 대여합니다.
                SessionPacket packet = _packet;
                packet.Init();

                byte[] totalSizeBytes = BitConverter.GetBytes(totalSize);
                byte[] messageSizeBytes = BitConverter.GetBytes(messageSize);

                // binary가 이미 byte[]인 것으로 가정
                packetLength = (ushort)(totalSizeBytes.Length + messageSizeBytes.Length + binary.Length);
                packet.SetLength(packetLength);

                if (packet != null && packet.Buffer != null)
                {
                    totalSizeBytesLength = (ushort)totalSizeBytes.Length;
                    messageSizeBytesLength = (ushort)messageSizeBytes.Length;
                    binaryLength = (ushort)binary.Length;

                    // 대여한 메모리에 복사
                    Buffer.BlockCopy(totalSizeBytes, 0, packet.Buffer, 0, totalSizeBytes.Length);
                    Buffer.BlockCopy(messageSizeBytes, 0, packet.Buffer, totalSizeBytes.Length, messageSizeBytes.Length);
                    Buffer.BlockCopy(binary, 0, packet.Buffer, totalSizeBytes.Length + messageSizeBytes.Length, binary.Length);

                    session.TellConnectedSocket(Tcp.Write.Create(ByteString.CopyFrom(packet.Buffer, 0, packetLength)));
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"EnqueuePendingPacket messageName:{messageName} totalSizeBytes.Length: {totalSizeBytesLength}, messageSizeBytes.Length: {messageSizeBytesLength}, " +
                $"binary.Length: {binaryLength}, packet.Buffer.Length: {packetLength}", ex);

            return false;

        }
    }

    public bool Tell(SessionInfoHandler session, MessageWrapper message)
    {
        var messageName = message.PayloadCase.ToString();
        try
        {
            var requestBinary = message.ToByteArrayWithBuffer();
            message.MessageSize = requestBinary.Length;

            var payload = session.PacketEncrypt
                ? CryptographyHelper.EncryptPacket(requestBinary)
                : requestBinary;

            ushort messageSize = (ushort)requestBinary.Length;
            ushort totalSize = (ushort)(2 + payload.Length); // messagesize(2) + data
            ushort packetSize = (ushort)(2 + 2 + payload.Length); // totalsize(2) + messagesize(2) + data

            if (_packet == null)
                _packet = new SessionPacket();

            var packet = _packet;
            packet.Init();
            packet.SetLength(packetSize);

            // zero allocation
            Span<byte> buffer = packet.Buffer.AsSpan(0, packetSize);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(0, 2), totalSize);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(2, 2), messageSize);
            payload.AsSpan().CopyTo(buffer.Slice(4));

            session.TellConnectedSocket(Tcp.Write.Create(ByteString.CopyFrom(packet.Buffer, 0, packetSize)));

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Tell failed. messageName:{messageName}", ex);
            return false;
        }
    }

}
