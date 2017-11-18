namespace DCCCommon.Messages
{
    public class DiscoveryResponseMessage
    {
        public string IPAddress { get; set; }
        public int ListeningPort { get; set; }
        public int NodeConnectionNum { get; set; }
    }
}