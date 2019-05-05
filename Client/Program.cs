using ChatLibrary;
using ChatLibrary.Packets;
using System;
using System.Net;

namespace Client
{
    class Program
    {
        static ChatClient client;

        static void Main(string[] args)
        {
            client = new ChatClient(591, IPAddress.Parse("127.0.0.1"));
            string mode, message, name;
            client.PacketRecieved += ProcessPacket;
            client.Start();

            Console.WriteLine("--------------Client started--------------");
            Console.Write("Do you want to register (r) or to login (l): ");
            mode = Console.ReadLine();

            if (mode == "r")
            {
                Console.Write("Enter your name: ");
                name = Console.ReadLine();
                client.CurrentUser = new User(name, client.ClientPort, client.ClientIP);
                client.SendPacket(new RegistrationRequestPacket(client.CurrentUser));

                while (true)
                {
                    message = Console.ReadLine();

                    client.SendPacket(new MessagePacket(client.CurrentUser, message));
                }
            }
            else if (mode == "l")
            {
                //Implemention of logging system
            }
        }

        /// <summary>
        /// Method to process packets got from listening
        /// </summary>
        /// <param name="msg"></param>
        static void ProcessPacket(Packet packet)
        {
            if (packet is MessagePacket)
            {
                MessagePacket msg = packet as MessagePacket;

                if (msg.Sender.Name != client.CurrentUser.Name)
                    Console.WriteLine($"{msg.Sender.Name}: {msg.Text}");
            }

            if (packet is RegistrationResponsePacket)
            {
                RegistrationResponsePacket reg = packet as RegistrationResponsePacket;

                if (reg.Result == true && reg.Sender == client.CurrentUser)
                {
                    Console.WriteLine($"Registration succeed, message: {reg.RegistrationMessage}.");
                }
                else if (reg.Result == false && reg.Sender == client.CurrentUser)
                {
                    Console.WriteLine($"Registration faliled, message: {reg.RegistrationMessage}.");
                }
            }
        }
    }
}
