﻿using Akka.Actor;
using DummyClient.Socket;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            using (var system = ActorSystem.Create("MySystem"))
            {
                //var clientProps = Props.Create(() => new TelnetClient("127.0.0.1", 8081));
                var clientProps = Props.Create(() => new TelnetClient());
                var client = system.ActorOf(clientProps, "TelnetClient");

                while (true)
                {
                    var input = Console.ReadLine();
                    if (input.Equals("exit"))
                        break;

                    client.Tell(input);
                }

                system.Terminate().Wait();
            }
        }
    }
}