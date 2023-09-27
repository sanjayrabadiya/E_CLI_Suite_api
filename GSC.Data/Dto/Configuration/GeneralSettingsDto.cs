namespace GSC.Data.Dto.Configuration
{
    public class GeneralSettingsDto
    {
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string SignalrUrl { get; set; }
        public string Idle { get; set; }
        public string Timeout { get; set; }
        public string Ping { get; set; }
        public string FirebaseServerId { get; set; }
        public string FirebaseSenderId { get; set; }
        public string FirebaseUrl { get; set; }
    }
}