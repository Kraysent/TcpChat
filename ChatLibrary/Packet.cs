using System;
using System.Net;

namespace ChatLibrary
{
    public abstract class Packet
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

}
