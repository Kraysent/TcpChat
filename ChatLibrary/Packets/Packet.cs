using System;
using System.Net;

namespace ChatLibrary.Packets
{
    public abstract class Packet
    {
        //Somehow create an abstract const string Header
        protected const char DELIM = '|';

        public abstract User Sender { get; set; }

        public abstract override string ToString();

        /// <summary>
        /// [FOR CLIENT] Looks threw all types of packets and returns needed; in other cases throws an exception
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
                case RegistrationResponsePacket.Header:
                    return new RegistrationResponsePacket(new User(split[1]), bool.Parse(split[2]), split[3]);
                default:
                    throw new ArgumentException("Can not convert string to Packet");
            }
        }

        /// <summary>
        /// [FOR SERVER] Looks threw all the types of packets and returns needed; in other cases throws an exception
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="senderIP"></param>
        /// <returns></returns>
        public static Packet Parse(string packet, IPAddress senderIP)
        {
            string[] split = packet.Split(DELIM);

            switch (split[0])
            {
                case MessagePacket.Header:
                    return new MessagePacket(new User(split[1], int.Parse(split[3]), senderIP), split[2]);
                case RegistrationRequestPacket.Header:
                    return new RegistrationRequestPacket(new User(split[1], int.Parse(split[2]), senderIP));
                default:
                    throw new ArgumentException("Can not convert string to Packet");
            }
        }
    }

}
