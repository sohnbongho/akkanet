using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Socket
{
    public sealed partial class TelnetClient : UntypedActor
    {
        private void SendCommnunityServerEnterRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                CommnunityServerEnterRequest = new CommnunityServerEnterRequest
                {
                    //SessionGuid = "3643b9f5-878d-4b80-b2a0-376c297bb99e",
                    SessionGuid = "c8621a9d-185c-4e99-9db8-d3ae12108cf6",
                }
            };
            Tell(request);
        }

    }
    
}
