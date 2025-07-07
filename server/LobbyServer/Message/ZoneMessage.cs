using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Message
{
    /// <summary>
    /// Zone 서버에서 온 메시지
    /// </summary>
    public class Z2LCMessage
    {
        /// <summary>
        /// 모든 존서버에 전달
        /// </summary>
        public class Broadcast
        {
            /// <summary>
            /// 메시지
            /// </summary>
            public MessageWrapper MessageWrapper { get; set; } = null!;
        }
    }
}
