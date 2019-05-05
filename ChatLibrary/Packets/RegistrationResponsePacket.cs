namespace ChatLibrary.Packets
{
    public class RegistrationResponsePacket : Packet
    {
        public const string Header = "RegistrationResponse";
        public override User Sender { get; set; }
        public bool Result { get; set; }
        public string RegistrationMessage { get; set; }

        public RegistrationResponsePacket(User sender, bool result, string registrationMessage)
        {
            Sender = sender;
            Result = result;
            RegistrationMessage = registrationMessage;
        }

        public override string ToString()
        {
            return $"{Header}{DELIM}{Sender.Name}{DELIM}{Result.ToString()}{DELIM}{RegistrationMessage}";
        }
    }
}
