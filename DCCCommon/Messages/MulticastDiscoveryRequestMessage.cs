namespace DCCCommon.Messages
{
    public class MulticastDiscoveryRequestMessage
    {
        public int ListeningPort { get; set; }
        public string IPAddress { get; set; }

        public override string ToString()
        {
            return $"{nameof(ListeningPort)}: {ListeningPort}, " +
                   $"{nameof(IPAddress)}: {IPAddress}";
        }
    }
}