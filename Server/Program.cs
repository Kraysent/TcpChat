using ChatLibrary;
using ChatLibrary.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        private static TcpListener _listener;
        private static List<User> _users;

        private static int _serverPort => 591;

        private delegate void Recieved(Packet packet);
        private static event Recieved PacketRecieved;

        static void Main(string[] args)
        {
            string message;

            PacketRecieved += ProcessPacket;

            _users = new List<User>();
            _listener = new TcpListener(IPAddress.Any, _serverPort);
            _listener.Start();
            RecievePackets();

            Console.WriteLine("--------------Server started--------------");

            while (true)
            {
                message = Console.ReadLine();

                SendPacket(new MessagePacket(new User("Server"), message));
            }
        }

        /// <summary>
        /// Continuously listenes for any new message to server port
        /// </summary>
        static async void RecievePackets()
        {
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                IPAddress clientIP = ((IPEndPoint)client.Client.LocalEndPoint).Address;
                NetworkStream stream = client.GetStream();
                Packet packet;
                string message = "";
                byte[] buffer = new byte[256];
                int i;

                do
                {
                    i = stream.Read(buffer, 0, buffer.Length);
                    message += Encoding.Default.GetString(buffer, 0, i);
                }
                while (stream.DataAvailable);

                stream.Close();

                packet = Packet.Parse(message, clientIP);

                PacketRecievedRaise(packet);
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
                
                if (UserRegistered(msg.Sender))
                {
                    Console.WriteLine($"{msg.Sender.Name}: {msg.Text}");

                    SendPacket(msg);
                }
            }

            if (packet is RegistrationRequestPacket)
            {
                RegistrationRequestPacket register = packet as RegistrationRequestPacket;

                if (!UserRegistered(register.Sender))
                {
                    RegisterUser(register.Sender);
                    Console.WriteLine($"New user {register.Sender.Name} connected to chat!");
                    SendPacket(new RegistrationResponsePacket(register.Sender, true, "Success."));
                    SendPacket(new MessagePacket(new User("Server"), $"New user {register.Sender.Name} connected to chat!"));
                }
            }
        }

        /// <summary>
        /// Sends packet for ALL registered clients
        /// </summary>
        /// <param name="packet"></param>
        static void SendPacket(Packet packet)
        {
            foreach (User user in _users)
            {
                TcpClient client = new TcpClient();
                NetworkStream stream;
                byte[] bytes;

                client.Connect(user.IP, user.Port);
                stream = client.GetStream();
                bytes = Encoding.Default.GetBytes(packet.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        
        /// <summary>
        /// Registers new user in the system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        static void RegisterUser(User user)
        {
            if (!UserRegistered(user))
            {
                _users.Add(user);
            }
        }

        /// <summary>
        /// Checks if user exists in system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        static bool UserRegistered(User user)
        {
            return _users.Any(x => x.Name == user.Name);
        }

        //------------------------------------------------------------//
        
        /// <summary>
        /// Raised only in RecieveMessages() method
        /// </summary>
        /// <param name="packet"></param>
        static void PacketRecievedRaise(Packet packet)
        {
            PacketRecieved?.Invoke(packet);
        }
    }
}
