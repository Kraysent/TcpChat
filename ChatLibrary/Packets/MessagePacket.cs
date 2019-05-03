namespace ChatLibrary.Packets
{
    public class MessagePacket : Packet
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
