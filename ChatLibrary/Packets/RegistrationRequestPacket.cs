using System;

namespace ChatLibrary.Packets
{
    public class RegistrationRequestPacket : Packet
    {
        public const string Header = "RegistrationRequest";
        public override User Sender { get; set; }

        public RegistrationRequestPacket(User sender)
        {
            Sender = sender;
        }

        public override string ToString()
        {
            return $"{Header}{DELIM}{Sender.Name}{DELIM}{Sender.Port}";
        }
    }
}
