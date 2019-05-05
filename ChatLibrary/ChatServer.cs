using ChatLibrary.Packets;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatLibrary
{
    public class ChatServer
    {
        private TcpListener _listener;

        public List<User> Users { get; private set; }

        public int ServerPort { get; private set; }

        public delegate void Recieved(Packet packet);
        public event Recieved PacketRecieved;

        public ChatServer(int serverPort)
        {
            ServerPort = serverPort;

            Users = new List<User>();
            _listener = new TcpListener(IPAddress.Any, ServerPort);
        }

        public void Start()
        {
            _listener.Start();
            RecievePackets();
        }
        
        /// <summary>
        /// Continuously listenes for any new message to server port
        /// </summary>
        private async void RecievePackets()
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
        /// Sends packet for ALL registered clients
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            foreach (User user in Users)
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
        public void RegisterUser(User user)
        {
            if (!UserRegistered(user))
            {
                Users.Add(user);
            }
        }

        /// <summary>
        /// Checks if user exists in system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UserRegistered(User user)
        {
            return Users.Any(x => x.Name == user.Name);
        }
        
        /// <summary>
        /// Raised only in RecievePackets() method
        /// </summary>
        /// <param name="packet"></param>
        private void PacketRecievedRaise(Packet packet)
        {
            PacketRecieved?.Invoke(packet);
        }
    }
}
