namespace DCCDiscoveryService.Messages
{
    public class DiscoveryResponseMessage
    {
        public int ListeningPort { get; set; }
        public string IPAddress { get; set; }
        public int NodeConnectionNum { get; set; }
    }
}