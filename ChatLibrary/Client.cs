using ChatLibrary.Packets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatLibrary
{
    public class ChatClient
    {
        private TcpListener _listener;
        private Random _rnd;

        public User CurrentUser { get; set; }

        public int ClientPort => ((IPEndPoint)_listener.LocalEndpoint).Port;
        public IPAddress ClientIP => ((IPEndPoint)_listener.LocalEndpoint).Address;
        public int ServerPort { get; private set; }
        public IPAddress ServerIP { get; private set; }
        
        public delegate void Recieved(Packet message);
        public event Recieved PacketRecieved;

        public ChatClient(int serverPort, IPAddress serverIP)
        {
            ServerPort = serverPort;
            ServerIP = serverIP;

            _rnd = new Random();
            _listener = new TcpListener(IPAddress.Any, _rnd.Next(10000));
        }

        public void Start()
        {
            _listener.Start();
            RecievePackets();
        }
        
        /// <summary>
        /// Method to send packets to the server
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream;
            byte[] bytes;

            client.Connect(ServerIP, ServerPort);
            stream = client.GetStream();
            bytes = Encoding.Default.GetBytes(packet.ToString());
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Method that continuously recieves packets from server (currently, not from server exactly, but from any IP)
        /// </summary>
        private async void RecievePackets()
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
        
        private void PacketRecievedRaise(Packet msg)
        {
            PacketRecieved?.Invoke(msg);
        }
    }
}
