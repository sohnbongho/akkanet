using Akka.Actor;
using Akka.IO;
using Google.Protobuf;
using Library.DTO;
using Library.Helper;
using Library.Helper.Encrypt;
using Messages;
using System.Net;

namespace DummyClient.Socket
{
    public sealed partial class TelnetClient : UntypedActor
    {
        public sealed class CmdHander
        {
            public string Name { get; set; }
            public Action<IActorRef> Handler { get; set; }
        }

        public sealed class EnterServer
        {
            public static EnterServer Instance { get; } = new();

            private EnterServer()
            {
            }
        }

        private IActorRef _connection;

        // TCP 특성상 다 오지 못 해, 버퍼가 쌓으면서 다 받으면 가져간다.
        private List<byte> _receivedBuffer = new List<byte>();
        private ushort? _totalReceivedMessageLength = null;
        private ushort? _messageLength = null;
        private const int _maxRecvLoop = 100; // 패킷받는 최대 카운트

        private string _userId = string.Empty; // userId        
        private bool _encrypt = true; // 암호화 사용

        private Dictionary<string, Action<IActorRef>> _userConnetedServers;

        private Dictionary<ServerType, Dictionary<string, CmdHander>> _typeToCmdHandlers = new();
        private ServerType _connectedServerType { get; set; } = ServerType.None;

        private Dictionary<string, CmdHander> _userLoginCmdHandlers;
        private Dictionary<string, CmdHander> _userLobbyCmdHandlers;
        private Dictionary<string, CmdHander> _userZoneCmdHandlers;
        private Dictionary<string, CmdHander> _userCommunityCmdHandlers;
        private Dictionary<string, CmdHander> _userRoomCmdHandlers;

        public TelnetClient()
        {
            _userConnetedServers = new Dictionary<string, Action<IActorRef>>
            {
                {"1", (sender) => OnRecvConntedLogin(sender)},
                {"2", (sender) => OnRecvConntedLobby(sender)},
                {"3", (sender) => OnRecvConntedZone(sender)},
                {"4", (sender) => OnRecvConntedCommunity(sender)},
                {"5", (sender) => OnRecvConntedRoom(sender)},
            };

            _userLoginCmdHandlers = new Dictionary<string, CmdHander>
            {
            };

            _userLobbyCmdHandlers = new Dictionary<string, CmdHander>
            {
            };

            _userZoneCmdHandlers = new Dictionary<string, CmdHander>
            {
                {"1", new CmdHander { Name = $"ZoneEnterRequest", Handler = ((sender) => SendZoneEnterRequest(sender))}},
                {"2", new CmdHander { Name = $"MoveRequest", Handler = ((sender) => SendMoveRequest(sender))}},
                {"3", new CmdHander { Name = $"ObjectUseRequest", Handler = ((sender) => SendObjectUseRequest(sender))}},
                {"4", new CmdHander { Name = $"ObjectUseEndRequest", Handler = ((sender) => SendObjectUseEndRequest(sender))}},
                {"5", new CmdHander { Name = $"ObjectStateRequest", Handler = ((sender) => SendObjectStateRequest(sender))}},
                {"6", new CmdHander { Name = $"ObjectOwnTypeRequest", Handler = ((sender) => SendObjectOwnTypeRequest(sender))}},
                {"7", new CmdHander { Name = $"ObjectTransformRequest", Handler = ((sender) => SendObjectTransformRequest(sender))}},
                {"8", new CmdHander { Name = $"PlayerActionRequest", Handler = ((sender) => SendPlayerActionRequest(sender))}},
                {"9", new CmdHander { Name = $"PlayerBallActionRequest", Handler = ((sender) => SendPlayerBallActionRequest(sender))}},
                {"10", new CmdHander { Name = $"InvenItemRequest", Handler = ((sender) => SendInvenItemRequest(sender))}},
                {"11", new CmdHander { Name = $"SysMessageRequest", Handler = ((sender) => SendSysMessageRequest(sender))}},
                {"12", new CmdHander { Name = $"RoomCreateRequest", Handler = ((sender) => SendRoomCreateRequest(sender))}},
                {"13", new CmdHander { Name = $"RoomListRequest", Handler = ((sender) => SendRoomListRequest(sender))}},
                {"14", new CmdHander { Name = $"RoomEnterRequest", Handler = ((sender) => SendRoomEnterRequest(sender))}},
                {"15", new CmdHander { Name = $"RoomLeaveRequest", Handler = ((sender) => SendRoomLeaveRequest(sender))}},
                {"16", new CmdHander { Name = $"RoomCharListRequest", Handler = ((sender) => SendRoomCharListRequest(sender))}},
                {"17", new CmdHander { Name = $"SellItemRequest", Handler = ((sender) => SendSellItemRequest(sender))}},
                {"18", new CmdHander { Name = $"ItemInvenExpandRequest", Handler = ((sender) => SendItemInvenExpandRequest(sender))}},
                {"20", new CmdHander { Name = $"RoomBuySeasonItemRequest", Handler = ((sender) => SendBuySeasonItemRequest(sender))}},
                {"21", new CmdHander { Name = $"RoomEquipSeasonItemRequest", Handler = ((sender) => SendEquipSeasonItemRequest(sender))}},
                {"22", new CmdHander { Name = $"RoomUnEquipSeasonItemRequest", Handler = ((sender) => SendUnEquipSeasonItemRequest(sender))}},
                {"23", new CmdHander { Name = $"ExchangeSnowForGoldRequest", Handler = ((sender) => SendExchangeSnowForGoldRequest(sender))}},
                {"24", new CmdHander { Name = $"KeepAlive", Handler = ((sender) => SendKeepAlive(sender))}},

            };
            _userCommunityCmdHandlers = new Dictionary<string, CmdHander>
            {
                //CommnunityServerEnterRequest
                {"1", new CmdHander { Name = $"CommnunityServerEnterRequest", Handler = ((sender) => SendCommnunityServerEnterRequest(sender))}},
            };
            _userRoomCmdHandlers = new();

            _typeToCmdHandlers.TryAdd(ServerType.Login, _userLoginCmdHandlers);
            _typeToCmdHandlers.TryAdd(ServerType.Lobby, _userLobbyCmdHandlers);
            _typeToCmdHandlers.TryAdd(ServerType.Zone, _userZoneCmdHandlers);
            _typeToCmdHandlers.TryAdd(ServerType.Community, _userCommunityCmdHandlers);
            _typeToCmdHandlers.TryAdd(ServerType.Room, _userRoomCmdHandlers);

        }
        protected override void PreStart()
        {
            base.PreStart();

            Console.WriteLine($"================================");
            Console.WriteLine($"1: Connect LogIn Server");
            Console.WriteLine($"2: Connect Lobby Server");
            Console.WriteLine($"3: Connect Zone Server");
            Console.WriteLine($"4: Connect Community Server");
            Console.WriteLine($"5: Connect Room Server");
            Console.WriteLine($"================================");
        }

        protected override void PostStop()
        {
            //Context.System.EventStream.Unsubscribe(Self, typeof(AssociationErrorEvent));
            //Context.System.EventStream.Unsubscribe(Self, typeof(DisassociatedEvent));

            // 생존성 모니터링 종료
            //_heartbeatTask?.Cancel();

            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Connected connected)
            {
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);

                // Register self as connection handler
                Sender.Tell(new Tcp.Register(Self));
                ReadConsoleAsync();
                Become(Connected(Sender));

                //var delay = TimeSpan.FromSeconds(3);
                //Context.System.Scheduler.ScheduleTellOnce(delay, Self, EnterServer.Instance, Self);

            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else if (message is string msg)
            {
                if (_connection != null)
                {
                    _connection.Tell(Tcp.Write.Create(Akka.IO.ByteString.FromString(msg + "\n")));
                }
                else
                {
                    // 서버 연결 시도
                    if (_userConnetedServers.TryGetValue(msg, out var handler))
                    {
                        handler(Sender);
                    }
                }

            }
            else
            {
                Unhandled(message);
            }
        }

        private UntypedReceive Connected(IActorRef connection)
        {
            _connection = connection;
            return message =>
            {
                if (message is Tcp.Received received)  // data received from network
                {
                    //Console.WriteLine(Encoding.ASCII.GetString(received.Data.ToArray()));                

                    _receivedBuffer.AddRange(received.Data.ToArray());

                    var intSize = sizeof(ushort);

                    // Loop while we might still have complete messages to process
                    for (var i = 0; i < _maxRecvLoop; ++i)
                    {
                        // If we don't know the length of the message yet (4 byte, int)
                        if (!_totalReceivedMessageLength.HasValue)
                        {
                            if (_receivedBuffer.Count < intSize)
                                return;

                            _totalReceivedMessageLength = BitConverter.ToUInt16(_receivedBuffer.ToArray(), 0);
                            _receivedBuffer.RemoveRange(0, intSize);
                        }
                        // decryption message size (4 byte, int)
                        if (!_messageLength.HasValue)
                        {
                            if (_receivedBuffer.Count < intSize)
                                return;

                            _messageLength = BitConverter.ToUInt16(_receivedBuffer.ToArray(), 0);
                            _receivedBuffer.RemoveRange(0, intSize);
                        }
                        // 메시지 크기
                        // 전체 패킷 사이즈 - decrpytionSize 사이즈
                        int encrypMessageSize = _totalReceivedMessageLength.Value - intSize;

                        // If entire message hasn't been received yet
                        if (_receivedBuffer.Count < encrypMessageSize)
                            return;

                        var messageSize = _messageLength.Value; // decrypt된 메시지 사이즈

                        // (암호화된)실제 메시지 읽기
                        var messageBytes = _receivedBuffer.GetRange(0, encrypMessageSize).ToArray();
                        _receivedBuffer.RemoveRange(0, encrypMessageSize);

                        var totalLength = _totalReceivedMessageLength.Value;

                        // 초기화
                        _totalReceivedMessageLength = null;
                        _messageLength = null;

                        // 패킷 암호화 사용중이면 decryp해주자
                        byte[] receivedMessage = null;
                        if (_encrypt)
                        {
                            receivedMessage = CryptographyHelper.DecryptPacket(messageBytes, messageSize);
                        }
                        else
                        {
                            receivedMessage = messageBytes;
                        }

                        // Handle the message
                        HandleMyMessage(receivedMessage);


                    }
                }
                else if (message is string s)   // data received from console
                {
                    //var request = new SayRequest
                    //{
                    //    UserName = "test",
                    //    Message = s
                    //};
                    if (false == _typeToCmdHandlers.TryGetValue(_connectedServerType, out var typeToCmdHandler))
                        return;

                    if (typeToCmdHandler.TryGetValue(s, out var handler))
                    {
                        Console.WriteLine($"{handler.Name}");
                        handler.Handler(Sender);
                    }
                    else
                    {
                        var request = new MessageWrapper
                        {
                            ZoneChatRequest = new ZoneChatRequest
                            {
                                Chat = s,
                            }
                        };
                        Tell(request);
                    }

                    ReadConsoleAsync();
                }
                else if (message is Tcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }

                else
                {
                    Unhandled(message);
                }
            };
        }

        private void OnRecvConntedLogin(IActorRef sender)
        {
            _connectedServerType = ServerType.Login;
            var endpoint = new DnsEndPoint("127.0.0.1", 16101);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }

        private void OnRecvConntedLobby(IActorRef sender)
        {
            _connectedServerType = ServerType.Lobby;
            var endpoint = new DnsEndPoint("127.0.0.1", 16201);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }
        private void OnRecvConntedZone(IActorRef sender)
        {
            _connectedServerType = ServerType.Zone;
            var endpoint = new DnsEndPoint("127.0.0.1", 16401);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }

        private void OnRecvConntedCommunity(IActorRef sender)
        {
            _connectedServerType = ServerType.Community;
            var endpoint = new DnsEndPoint("127.0.0.1", 16301);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }
        private void OnRecvConntedRoom(IActorRef sender)
        {
            _connectedServerType = ServerType.Room;
            var endpoint = new DnsEndPoint("127.0.0.1", 16501);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }

        private void ReadConsoleAsync()
        {
            if (false == _typeToCmdHandlers.TryGetValue(_connectedServerType, out var typeToCmdHandler))
            {
                Console.WriteLine($"not found type:{_connectedServerType}");
                return;
            }

            foreach (var handler in typeToCmdHandler)
            {
                Console.WriteLine($"{handler.Key}:{handler.Value.Name}");
            }
            Console.WriteLine($"");

            Task.Factory.StartNew(self => Console.In.ReadLineAsync().PipeTo((ICanTell)self), Self);
        }
        private void Tell(MessageWrapper request)
        {
            var json = PacketLogHelper.Instance.GetLogJson(request);
            Console.WriteLine("");
            Console.WriteLine($"Client->Server - type({request.PayloadCase}) data({json})");
            Console.WriteLine("");

            var requestBinary = request.ToByteArray();
            request.MessageSize = requestBinary.Length;

            ushort totalSize = sizeof(ushort);
            ushort messageSize = (ushort)requestBinary.Length;

            byte[] binary = null;

            if (_encrypt)
            {
                binary = CryptographyHelper.EncryptPacket(requestBinary);
                totalSize += (ushort)binary.Length;
            }
            else
            {
                binary = requestBinary;
                totalSize += (ushort)requestBinary.Length;
            }

            byte[] byteArray = null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(totalSize); // size는 int으로                
                    writer.Write(messageSize);
                    writer.Write(binary);

                    byteArray = stream.ToArray();
                }
            }
            if (byteArray != null)
                _connection.Tell(Tcp.Write.Create(Akka.IO.ByteString.FromBytes(byteArray)));

        }
        private bool HandleMyMessage(byte[] recvBuffer)
        {
            var receivedMessage = recvBuffer;

            // 전체를 관리하는 wapper로 변환 역직렬화
            var wrapper = MessageWrapper.Parser.ParseFrom(receivedMessage);
            var json = PacketLogHelper.Instance.GetLogJson(wrapper);
            Console.WriteLine($"Client<-Server - type({wrapper.PayloadCase}) data({json})");
            Console.WriteLine("");

            switch (wrapper.PayloadCase)
            {
                case MessageWrapper.PayloadOneofCase.ConnectedResponse:
                    {
                        //// 서버에 입장
                        //var request = new MessageWrapper
                        //{
                        //    LoginDirectRequest = new LoginDirectRequest
                        //    {
                        //        AccountId = "crazy1",
                        //        Pass = "crazy",
                        //    }
                        //};
                        //Tell(request);
                        break;
                    }
                case MessageWrapper.PayloadOneofCase.ZoneChatNoti:
                    {
                        break;
                    }
            }
            return true;
        }
    }
}

