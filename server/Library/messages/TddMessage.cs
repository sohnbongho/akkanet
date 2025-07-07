using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.messages
{
    /// <summary>
    /// TDD 테스트 관련 메시지 처리
    /// </summary>
    public class TDDMessage
    {
        public class Message
        {
            public MessageWrapper MessageWrapper { get; set; } = null!;
        }

    }
}
