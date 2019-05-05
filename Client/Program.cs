using ChatLibrary;
using ChatLibrary.Packets;
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
            string message, name, mode;

            PacketRecieved += ProcessPacket;

            _rnd = new Random();
            _listener = new TcpListener(IPAddress.Any, _rnd.Next(10000));
            _listener.Start();
            RecievePackets();

            Console.WriteLine("--------------Client started--------------");
            Console.Write("Do you want to register (r) or to login (l): ");
            mode = Console.ReadLine();

            if (mode == "r")
            {
                Console.Write("Enter your name: ");
                name = Console.ReadLine();
                _currentUser = new User(name, _clientPort, _clientIP);
                SendPacket(new RegistrationRequestPacket(_currentUser));
                
                while (true)
                {
                    message = Console.ReadLine();

                    SendPacket(new MessagePacket(_currentUser, message));
                }
            }
            else if (mode == "l")
            {
                //Implemention of logging system
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
                PacketRecievedRaise(packet);
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

            if (packet is RegistrationResponsePacket)
            {
                RegistrationResponsePacket reg = packet as RegistrationResponsePacket;

                if (reg.Result == true && reg.Sender == _currentUser)
                {
                    Console.WriteLine($"Registration succeed, message: {reg.RegistrationMessage}.");
                }
                else if (reg.Result == false && reg.Sender == _currentUser)
                {
                    Console.WriteLine($"Registration faliled, message: {reg.RegistrationMessage}.");
                }
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
        static void PacketRecievedRaise(Packet msg)
        {
            PacketRecieved?.Invoke(msg);
        }
    }
}
