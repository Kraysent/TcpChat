using System.Collections.Generic;
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

        public override bool Equals(object obj)
        {
            var user = obj as User;
            return user != null &&
                   Name == user.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(User user1, User user2)
        {
            return EqualityComparer<User>.Default.Equals(user1, user2);
        }

        public static bool operator !=(User user1, User user2)
        {
            return !(user1 == user2);
        }
    }
}
