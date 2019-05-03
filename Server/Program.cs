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

                if (RegisterUser(msg.Sender) == false)
                {
                    Console.WriteLine($"{msg.Sender.Name} connected to chat!");
                    SendPacket(new MessagePacket(new User("Server"), $"{msg.Sender.Name} connected to chat!"));
                }

                Console.WriteLine($"{msg.Sender.Name}: {msg.Text}");

                SendPacket(msg);
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
        static bool RegisterUser(User user)
        {
            bool userRegistered = _users.Select(x => x.Name).Contains(user.Name);

            if (!userRegistered)
            {
                _users.Add(user);
            }

            return userRegistered;
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
    
    class User
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public IPAddress IP { get; set; }

        public User(string name, int port, IPAddress ip)
        {
            Name = name;
            Port = port;
            IP = ip;
        }

        public User(string name)
        {
            Name = name;
        }
    }
    
    abstract class Packet
    {
        protected const char DELIM = '|';

        public abstract override string ToString();

        public static Packet Parse(string packet, IPAddress senderIP)
        {
            string[] split = packet.Split(DELIM);

            switch (split[0])
            {
                case MessagePacket.Header:
                    return new MessagePacket(new User(split[1], int.Parse(split[3]), senderIP), split[2]);
                default:
                    throw new ArgumentException("Can not convert string to Packet");
            }
        }
    }

    class MessagePacket : Packet
    {
        public const string Header = "Message";
        public User Sender { get; set; }
        public string Text { get; set; }

        public MessagePacket(User sender, string text)
        {
            Sender = sender;
            Text = text;
        }

        public override string ToString()
        {
            if (Sender.Port == 0)
                return $"{Header}{DELIM}{Sender.Name}{DELIM}{Text}";
            else
                return $"{Header}{DELIM}{Sender.Name}{DELIM}{Text}{DELIM}{Sender.Port}";
        }
    }
}
