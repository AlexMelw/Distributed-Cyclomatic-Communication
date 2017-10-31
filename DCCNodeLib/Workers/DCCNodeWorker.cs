namespace DCCNodeLib.Workers
{
    using System.Net;
    using System.Threading.Tasks;
    using Interfaces;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        public IPAddress LocalIpAddress { get; set; }

        public IPEndPoint MulticastIPEndPoint { get; set; }

        public Task StartAsync()
        {
            StartListeningToMulticastPortAsync();
            StartListeningToTcpServingPortAsync();
        }

        private Task StartListeningToMulticastPortAsync()
        {
            //while (true)
            //{
                
            //}
        }

        private Task StartListeningToTcpServingPortAsync()
        {
            //while (true)
            //{
                
            //}
        }

        public void Dispose() { }
    }
}