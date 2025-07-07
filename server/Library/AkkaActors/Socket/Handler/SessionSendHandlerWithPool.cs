using Akka.IO;
using Library.Helper.Encrypt;
using Library.Memory;
using Library.Memory.ProtoBuffer;
using Library.messages;
using log4net;
using Messages;
using System.Reflection;

namespace Library.AkkaActors.Socket.Handler;

public class SessionSendHandlerWithPool : IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // TCP 패킷 보내는 것에 대한 처리
    private SessionPacket? _sendingPacket = null;
    private byte[]? _sendingPacketBytes = null;

    private int _sendingBytes = 0; // 보내고 있는 패킷
    private int _sendedBytes = 0;  // 보낸 패킷        
    private Queue<SessionPacket>? _sendQueue = null;
    // 메모리 풀
    private PacketMemoryPool<SessionPacket>? _sendMemoryPool = null;

    public bool IsCompleted() => _sendedBytes >= _sendingBytes;
    public void Init()
    {
        _sendingPacket = null;

        _sendingPacketBytes = null;

        _sendQueue = new Queue<SessionPacket>();
        _sendMemoryPool = new PacketMemoryPool<SessionPacket>();
    }
    public void Dispose()
    {
        if (_sendingPacket != null)
        {
            _sendingPacket.Dispose();
            _sendingPacket = null;
        }

        _sendingPacketBytes = null;

        if (_sendQueue != null)
        {
            while (_sendQueue.Count > 0)
            {
                var packet = _sendQueue.Dequeue();
                if (packet is IDisposable)
                {
                    packet.Dispose();
                }
            }
            _sendQueue.Clear();
            _sendQueue = null;
        }
        if (_sendMemoryPool != null)
        {
            _sendMemoryPool.Dispose();
            _sendMemoryPool = null;
        }
    }

    public (bool, object) DequeuePendingPacket()
    {
        if (_sendedBytes >= _sendingBytes)
        {
            // 다 보냈으면 다음 큐에서 가져온다.
            if (TrySendNextMessage() == false)
            {
                // 큐에도 없으면 보낼것이 없는 것이다.
                return (false, new object());
            }
        }
        try
        {
            // 남은 패킷 조각 계산
            int remainingBytes = _sendingBytes - _sendedBytes;

            // 전송할 패킷 조각 계산
            int chunkSize = Math.Min(remainingBytes, SessionPacket.MaxBufferBytes); // 조각 크기는 적절히 조정 가능

            // 패킷 조각 생성
            var chunk = Akka.IO.ByteString.FromBytes(_sendingPacketBytes, _sendedBytes, chunkSize);

            // 전송한 바이트 수 업데이트
            _sendedBytes += chunkSize;

            // 패킷 조각 전송            
            if (chunk == null)
            {
                return (false, new object());
            }

            var sendObject = Tcp.Write.Create(chunk, Session.Ack.Instance);
            return (true, sendObject);
        }
        catch (Exception ex)
        {
            _logger.Error($"DequeuePendingPacket", ex);
            return (false, new object());
        }
    }

    /// <summary>
    /// 큐에서 다음 메시지를 가져와 보냅니다.
    /// </summary>

    private bool TrySendNextMessage()
    {
        if (_sendQueue == null)
        {
            _sendQueue = new Queue<SessionPacket>();
        }
        if (_sendMemoryPool == null)
        {
            _sendMemoryPool = new PacketMemoryPool<SessionPacket>();
        }

        // 큐에서 다음 메시지를 가져와 보냅니다.
        if (_sendQueue.Count <= 0)
            return false;

        try
        {
            if (_sendingPacket != null)
            {
                // 전에 메모리 풀에서 가져온 버퍼 반환
                _sendMemoryPool.Return(_sendingPacket);
            }

            _sendingPacket = _sendQueue.Dequeue();
            _sendingBytes = _sendingPacket.Length;
            _sendingPacketBytes = new byte[_sendingBytes];
            if (_sendingPacket != null && _sendingPacket.Buffer != null)
            {
                Array.Copy(_sendingPacket.Buffer, _sendingPacketBytes, _sendingBytes);
            }

            _sendedBytes = 0;

            return (_sendedBytes < _sendingBytes);
        }
        catch (Exception ex)
        {
            _logger.Error($"TrySendNextMessage", ex);
            return false;
        }
    }

    public void EnqueuePendingPacket(MessageWrapper message, bool packetEncrypt)
    {
        //int threadId = Thread.CurrentThread.ManagedThreadId;
        //var json = PacketLogHelper.Instance.GetLogJson(message);
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) data({json})");
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) ");

        if (_sendQueue == null)
        {
            _sendQueue = new Queue<SessionPacket>();
        }
        if (_sendMemoryPool == null)
        {
            _sendMemoryPool = new PacketMemoryPool<SessionPacket>();
        }

        var messageName = message.PayloadCase.ToString();
        ushort packetLength = 0;

        ushort totalSizeBytesLength = 0;
        ushort messageSizeBytesLength = 0;
        ushort binaryLength = 0;

        try
        {
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

            {
                // 메모리 풀에서 필요한 만큼의 메모리를 대여합니다.
                SessionPacket packet = _sendMemoryPool.Rent();
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

                    _sendQueue.Enqueue(packet);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"EnqueuePendingPacket messageName:{messageName} totalSizeBytes.Length: {totalSizeBytesLength}, messageSizeBytes.Length: {messageSizeBytesLength}, " +
                $"binary.Length: {binaryLength}, packet.Buffer.Length: {packetLength}", ex);

        }
    }
    public void EnqueuePendingPacket2(MessageWrapper message, bool packetEncrypt)
    {
        //int threadId = Thread.CurrentThread.ManagedThreadId;
        //var json = PacketLogHelper.Instance.GetLogJson(message);
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) data({json})");
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) ");

        if (_sendQueue == null)
        {
            _sendQueue = new Queue<SessionPacket>();
        }
        if (_sendMemoryPool == null)
        {
            _sendMemoryPool = new PacketMemoryPool<SessionPacket>();
        }

        try
        {
            byte[] requestBinary = message.ToByteArrayWithPool();
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

            {
                // 메모리 풀에서 필요한 만큼의 메모리를 대여합니다.
                SessionPacket packet = _sendMemoryPool.Rent();
                packet.Init();

                byte[] totalSizeBytes = BitConverter.GetBytes(totalSize);
                byte[] messageSizeBytes = BitConverter.GetBytes(messageSize);

                // binary가 이미 byte[]인 것으로 가정                
                var packetLength = (ushort)(totalSizeBytes.Length + messageSizeBytes.Length + binary.Length);
                packet.SetLength(packetLength);

                if (packet != null && packet.Buffer != null)
                {
                    // 대여한 메모리에 복사
                    Buffer.BlockCopy(totalSizeBytes, 0, packet.Buffer, 0, totalSizeBytes.Length);
                    Buffer.BlockCopy(messageSizeBytes, 0, packet.Buffer, totalSizeBytes.Length, messageSizeBytes.Length);
                    Buffer.BlockCopy(binary, 0, packet.Buffer, totalSizeBytes.Length + messageSizeBytes.Length, binary.Length);

                    _sendQueue.Enqueue(packet);
                }
            }

        }
        catch (Exception ex)
        {
            _logger.Error($"SessionActor Tell", ex);

        }
    }

}
