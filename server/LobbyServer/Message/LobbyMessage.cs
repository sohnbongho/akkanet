using Akka.Actor;
using Akka.IO;
using Library.DTO;
using Messages;
using NatsMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Message
{
    /// <summary>
    /// User, UserCordiator 간 메시지
    /// </summary>
    public class U2UCMessage
    {
        public class EnterLobbyRequest
        {
            public ulong UserSeq { get; set; }          
            public string RemoteAddress { get; set; } = null!;
        }
        
    }

    
}
