using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        private static TcpListener _listener;
        private static Random _rnd;
        private static User _currentUser;

        private static int _clientPort => ((IPEndPoint)_listener.LocalEndpoint).Port;
        private static IPAddress _clientIP => ((IPEndPoint)_listener.LocalEndpoint).Address;
        private static int _serverPort => 591;
        private static IPAddress _serverIP => IPAddress.Parse("127.0.0.1");
        
        private delegate void Recieved(Packet message);
        private static event Recieved PacketRecieved;

        static void Main(string[] args)
        {
            string message, name;

            PacketRecieved += ProcessPacket;

            _rnd = new Random();
            _listener = new TcpListener(IPAddress.Any, _rnd.Next(10000));
            _listener.Start();
            RecievePackets();

            Console.Write("Enter your name: ");
            name = Console.ReadLine();
            _currentUser = new User(name, _clientPort, _clientIP);
            
            while (true)
            {
                message = Console.ReadLine();

                SendPacket(new MessagePacket(_currentUser, message));
            }
        }

        /// <summary>
        /// Method that continuously recieves packets from server (currently, not from server exactly, but from any IP)
        /// </summary>
        static async void RecievePackets()
        {
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
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

                packet = Packet.Parse(message);

                if (packet is MessagePacket)
                {
                    MessageRecievedRaise(packet as MessagePacket);
                }
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

                if (msg.Sender.Name != _currentUser.Name)
                    Console.WriteLine($"{msg.Sender.Name}: {msg.Text}");
            }
        }
        
        /// <summary>
        /// Method to send packets (only to server)
        /// </summary>
        /// <param name="packet"></param>
        static void SendPacket(Packet packet)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream;
            byte[] bytes;
            
            client.Connect(_serverIP, _serverPort);
            stream = client.GetStream();
            bytes = Encoding.Default.GetBytes(packet.ToString());
            stream.Write(bytes, 0, bytes.Length);
        }

        //------------------------------------------------------------//
        
        /// <summary>
        /// Raised only in RecievePackets() method
        /// </summary>
        /// <param name="msg"></param>
        static void MessageRecievedRaise(Packet msg)
        {
            PacketRecieved?.Invoke(msg);
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

        /// <summary>
        /// Looks threw all types of packets and returns needed; in other cases throws an exception
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static Packet Parse(string packet)
        {
            string[] split = packet.Split(DELIM);

            switch (split[0])
            {
                case MessagePacket.Header:
                    return new MessagePacket(new User(split[1]), split[2]);
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
