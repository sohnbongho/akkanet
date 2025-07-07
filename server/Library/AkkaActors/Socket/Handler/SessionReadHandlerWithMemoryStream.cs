using Akka.Actor;
using Library.Helper.Encrypt;
using Library.messages;
using log4net;
using Messages;
using System.Reflection;

namespace Library.AkkaActors.Socket.Handler;

public class SessionReadHandlerWithMemoryStream : IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // 패킷 읽는 부분        
    private ushort? _totalReceivedMessageLength = null;
    private ushort? _messageLength = null;
    private const int _maxRecvLoop = 100; // 패킷받는 최대 카운트            

    // MemoryStream방식
    private MemoryStream? _receivedStream = new();
    private int _receivedPosition = 0;
    private byte[]? _remainingData = null;
    private byte[]? _lengthBytes = null;
    private long _remainingDataSize = SessionPacket.MaxBufferBytes;

    public void Init()
    {
        _totalReceivedMessageLength = null;
        _messageLength = null;

        _receivedStream = new MemoryStream();
        _receivedPosition = 0;

        _remainingData = new byte[_remainingDataSize];
        _lengthBytes = new byte[SessionPacket.UShortSize];
    }
    public void Dispose()
    {
        _totalReceivedMessageLength = null;
        _messageLength = null;

        if (_receivedStream != null)
        {
            _receivedStream.Dispose();
            _receivedStream = null;
        }

        _remainingData = null;
        _lengthBytes = null;
    }

    public bool HandleReceived(byte[] data, bool packetEncrypt, IActorRef self)
    {
        try
        {
            if (_receivedStream == null)
            {
                _receivedStream = new MemoryStream();
            }
            if (_lengthBytes == null)
            {
                _lengthBytes = new byte[SessionPacket.UShortSize];
            }
            if (_remainingData == null)
            {
                _remainingData = new byte[_remainingDataSize];
            }

            if (_receivedStream.Length > 0)
            {
                // 계속 받고 있는 중으므로 마지막으로 보내자
                _receivedStream.Position = _receivedStream.Length;
            }

            // 받은 데이터를 메모리 스트림에 씁니다.
            _receivedStream.Write(data, 0, data.Length);
            _receivedStream.Position = _receivedPosition; // 헤더를 읽었으면 그 다음부터 읽자

            // Loop while we might still have complete messages to process
            for (var i = 0; i < _maxRecvLoop; ++i)
            {
                // If we don't know the length of the message yet (2 bytes, ushort)
                if (!_totalReceivedMessageLength.HasValue)
                {
                    if (_receivedStream.Length - _receivedStream.Position < sizeof(ushort))
                        return true;

                    _receivedStream.Read(_lengthBytes, 0, sizeof(ushort));
                    // _receivedStream.Position 값 2로 됨
                    _receivedPosition += sizeof(ushort);

                    _totalReceivedMessageLength = BitConverter.ToUInt16(_lengthBytes, 0);
                }

                // Decryption message size (2 bytes, ushort)
                if (!_messageLength.HasValue)
                {
                    if (_receivedStream.Length - _receivedStream.Position < sizeof(ushort))
                        return true;

                    _receivedStream.Read(_lengthBytes, 0, sizeof(ushort));
                    // _receivedStream.Position 값 4로 됨
                    _receivedPosition += sizeof(ushort);

                    _messageLength = BitConverter.ToUInt16(_lengthBytes, 0);
                }

                if (_totalReceivedMessageLength.Value < sizeof(ushort))
                {
                    throw new InvalidOperationException($"Invalid packet length:{_totalReceivedMessageLength.Value}.");
                }

                int encrypMessageSize = checked(_totalReceivedMessageLength.Value - sizeof(ushort));

                // If entire message hasn't been received yet
                if (_receivedStream.Length - _receivedPosition < encrypMessageSize)
                    return true;

                var messageSize = _messageLength.Value;
                byte[] messageBytes = new byte[encrypMessageSize];
                _receivedStream.Read(messageBytes, 0, encrypMessageSize);
                _receivedPosition += encrypMessageSize;

                // 초기화
                _totalReceivedMessageLength = null;
                _messageLength = null;

                // 패킷 암호화 unpack
                byte[] receivedMessage = packetEncrypt
                    ? CryptographyHelper.DecryptPacket(messageBytes, messageSize)
                    : messageBytes;

                // Handle the message
                OnRecvPacket(messageSize, receivedMessage, self);

                // 데이터가 남았는가?
                long bytesToKeep = _receivedStream.Length - _receivedStream.Position;
                if (bytesToKeep > 0)
                {
                    if (bytesToKeep > _remainingDataSize)
                    {
                        _remainingDataSize = bytesToKeep;
                        _remainingData = new byte[_remainingDataSize];
                    }
                    else
                    {
                        Array.Clear(_remainingData);
                    }

                    _receivedStream.Read(_remainingData, 0, (int)bytesToKeep);

                    // 데이터를 초기화
                    _receivedStream.SetLength(0);
                    _receivedPosition = 0;

                    _receivedStream.Write(_remainingData, 0, (int)bytesToKeep);
                    _receivedStream.Position = 0;
                }
                else
                {
                    // 모든 데이터 읽기 성공
                    // 데이터를 초기화
                    _receivedStream.SetLength(0);
                    _receivedStream.Position = 0;
                    _receivedPosition = 0;
                }
            }
            return true;
        }
        catch (OverflowException ex)
        {
            _logger.Error($"Arithmetic overflow in HandleReceived: {ex.Message}");
            return false;

        }
        catch (InvalidOperationException ex)
        {
            _logger.Error($"Invalid packet data in HandleReceived: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error($"failed to HandleReceived", ex);
            return false;
        }
    }

    /// <summary>
    /// 클라이언트 패킷 통신
    /// </summary>    
    private bool OnRecvPacket(ushort messageSize, byte[] receivedMessage, IActorRef self)
    {
        var wrapperName = string.Empty;
        try
        {
            // 전체를 관리하는 wapper로 변환 역직렬화
            MessageWrapper wrapper = MessageWrapper.Parser.ParseFrom(receivedMessage);
            //var json = PacketLogHelper.Instance.GetLogJson(wrapper);
            //_logger.DebugEx(()=>$"Server<-Client - ip({_remoteAddress}) seq({SessionCharSeq}) type({wrapperName}) data({json})");
            //_logger.DebugEx(()=>$"Server<-Client - ip({_remoteAddress}) seq({SessionCharSeq}) type({wrapperName}) ");

            self.Tell(wrapper);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"OnRecvPacket {wrapperName}", ex);
            return false;
        }
    }
}
