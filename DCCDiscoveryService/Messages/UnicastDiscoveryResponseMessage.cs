namespace DCCDiscoveryService.Messages
{
    public class UnicastDiscoveryResponseMessage
    {
        public int ListeningPort { get; set; }
        public string IPAddress { get; set; }
        public int NodeConnectionNum { get; set; }
    }
}