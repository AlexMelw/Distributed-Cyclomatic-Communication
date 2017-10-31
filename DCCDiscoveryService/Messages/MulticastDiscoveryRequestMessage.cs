namespace DCCDiscoveryService.Messages
{
    public class MulticastDiscoveryRequestMessage
    {
        public int ListeningPort { get; set; }
        public string IPAddress { get; set; }
    }
}