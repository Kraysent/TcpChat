using System.Net;

namespace ChatLibrary
{
    public class User
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

}
