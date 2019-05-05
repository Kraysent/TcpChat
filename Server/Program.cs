using ChatLibrary;
using ChatLibrary.Packets;
using System;

namespace Server
{
    class Program
    {
        static ChatServer Server;

        static void Main(string[] args)
        {
            string message;

            Server = new ChatServer(591);
            Server.PacketRecieved += ProcessPacket;
            Server.Start();

            Console.WriteLine("--------------Server started--------------");

            while (true)
            {
                message = Console.ReadLine();

                Server.SendPacket(new MessagePacket(new User("Server"), message));
            }
        }
        
        /// <summary>
        /// Method to process messages got from any client
        /// </summary>
        /// <param name="packet"></param>
        static void ProcessPacket(Packet packet)
        {
            if (packet is MessagePacket)
            {
                MessagePacket msg = packet as MessagePacket;
                
                if (Server.UserRegistered(msg.Sender))
                {
                    Console.WriteLine($"{msg.Sender.Name}: {msg.Text}");

                    Server.SendPacket(msg);
                }
            }

            if (packet is RegistrationRequestPacket)
            {
                RegistrationRequestPacket register = packet as RegistrationRequestPacket;

                if (!Server.UserRegistered(register.Sender))
                {
                    Server.RegisterUser(register.Sender);
                    Console.WriteLine($"New user {register.Sender.Name} connected to chat!");
                    Server.SendPacket(new RegistrationResponsePacket(register.Sender, true, "Success."));
                    Server.SendPacket(new MessagePacket(new User("Server"), $"New user {register.Sender.Name} connected to chat!"));
                }
            }
        }
    }
}
