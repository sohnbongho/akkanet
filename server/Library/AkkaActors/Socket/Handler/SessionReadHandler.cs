using Akka.Actor;
using Library.Helper.Encrypt;
using Library.messages;
using log4net;
using Messages;

using System.Reflection;


namespace Library.AkkaActors.Socket.Handler;


public class SessionReadHandler : IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // TCP 특성상 다 오지 못 해, 버퍼가 쌓으면서 다 받으면 가져간다.
    private List<byte>? _receivedBuffer = null;
    private ushort? _totalReceivedMessageLength = null;
    private ushort? _messageLength = null;
    private const int _maxRecvLoop = 100; // 패킷받는 최대 카운트    
    private string _lastPacketName = string.Empty; // 마지막에 받은 내용

    public void Init()
    {
        _totalReceivedMessageLength = null;
        _messageLength = null;
        _receivedBuffer = null;

    }

    public void Dispose()
    {
        if(_receivedBuffer != null )
        {
            _receivedBuffer.Clear();
            _receivedBuffer = null;
        }        

        _totalReceivedMessageLength = null;
        _messageLength = null;
    }


    public bool HandleReceived(byte[] receivedData, bool encrypt, IActorRef self, string remoteAddress, ulong sessionUserSeq)
    {
        try
        {
            if (_receivedBuffer == null)
            {
                _receivedBuffer = new List<byte>(SessionPacket.MaxBufferBytes);
            }

            _receivedBuffer.AddRange(receivedData);

            const int intSize = sizeof(ushort);

            // Loop while we might still have complete messages to process
            for (var i = 0; i < _maxRecvLoop; ++i)
            {
                // If we don't know the length of the message yet (4 byte, int)
                if (!_totalReceivedMessageLength.HasValue)
                {
                    if (_receivedBuffer.Count < intSize)
                        return true;

                    _totalReceivedMessageLength = BitConverter.ToUInt16(_receivedBuffer.ToArray(), 0);
                    _receivedBuffer.RemoveRange(0, intSize);
                }
                // decryption message size (4 byte, int)
                if (!_messageLength.HasValue)
                {
                    if (_receivedBuffer.Count < intSize)
                        return true;

                    _messageLength = BitConverter.ToUInt16(_receivedBuffer.ToArray(), 0);
                    _receivedBuffer.RemoveRange(0, intSize);
                }
                // 메시지 크기
                // 전체 패킷 사이즈 - decrpytionSize 사이즈
                int encrypMessageSize = _totalReceivedMessageLength.Value - intSize;

                // If entire message hasn't been received yet
                if (_receivedBuffer.Count < encrypMessageSize)
                    return true;

                var messageSize = _messageLength.Value; // decrypt된 메시지 사이즈

                if (encrypMessageSize <= 0)
                {
                    _logger.Error($"Invalid Packet. sessionUserSeq:{sessionUserSeq} LastPacketName:{_lastPacketName} encrypMessageSize: {encrypMessageSize}, totalLength: {_totalReceivedMessageLength}, intSize: {intSize} ");
                    // 초기화
                    _totalReceivedMessageLength = null;
                    _messageLength = null;

                    return false;
                }

                // (암호화된)실제 메시지 읽기
                var messageBytes = _receivedBuffer.GetRange(0, encrypMessageSize).ToArray();
                _receivedBuffer.RemoveRange(0, encrypMessageSize);

                var totalLength = _totalReceivedMessageLength.Value;

                // 초기화
                _totalReceivedMessageLength = null;
                _messageLength = null;

                // 패킷 암호화 사용중이면 decryp해주자
                byte[] receivedMessage = null!;
                if (encrypt)
                {
                    receivedMessage = CryptographyHelper.DecryptPacket(messageBytes, messageSize);
                }
                else
                {
                    receivedMessage = messageBytes;
                }

                // Handle the message
                HandleMyMessage(receivedMessage, self, remoteAddress, sessionUserSeq);
            }
            return true;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.Error($"ArgumentOutOfRangeException in remoteAddress:{remoteAddress} sessionUserSeq:{sessionUserSeq} LastPacketName:{_lastPacketName} HandleReceived: {ex.Message}", ex);
            return false;

        }
        catch (OverflowException ex)
        {
            _logger.Error($"Arithmetic overflow in remoteAddress:{remoteAddress} sessionUserSeq:{sessionUserSeq} LastPacketName:{_lastPacketName} HandleReceived: {ex.Message}", ex);
            return false;

        }
        catch (InvalidOperationException ex)
        {
            _logger.Error($"Invalid packet data in remoteAddress:{remoteAddress} sessionUserSeq:{sessionUserSeq} LastPacketName:{_lastPacketName} HandleReceived: {ex.Message}", ex);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error($"failed to HandleReceived. remoteAddress:{remoteAddress} sessionUserSeq:{sessionUserSeq} LastPacketName:{_lastPacketName} ", ex);
            return false;
        }
    }

    private bool HandleMyMessage(byte[] recvBuffer, IActorRef self, string remoteAddress, ulong sessionUserSeq)
    {
        var packetLength = recvBuffer.Length;

        try
        {
            // 전체를 관리하는 wapper로 변환 역직렬화
            var wrapper = MessageWrapper.Parser.ParseFrom(recvBuffer);
            self.Tell(wrapper);

            _lastPacketName = wrapper.PayloadCase.ToString();

            return true;

        }
        catch (Exception ex)
        {
            _logger.Error($"OnRecvPacket remoteAddress:{remoteAddress} sessionUserSeq:{sessionUserSeq} size:{packetLength} LastPacketName:{_lastPacketName}", ex);
            return false;
        }
    }

}
