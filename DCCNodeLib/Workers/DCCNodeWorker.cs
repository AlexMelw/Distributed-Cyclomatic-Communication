namespace DCCNodeLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Interfaces;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        public IPAddress LocalIpAddress { get; set; }

        public IPEndPoint MulticastIPEndPoint { get; set; }

        public int TcpServingPort { get; set; }

        public Task StartAsync()
        {
            //StartListeningToMulticastPortAsync();
            //StartListeningToTcpServingPortAsync();

            throw new NotImplementedException();
        }

        private Task StartListeningToMulticastPortAsync()
        {
            //while (true)
            //{

            //}

            throw new NotImplementedException();
        }

        private Task StartListeningToTcpServingPortAsync()
        {
            //while (true)
            //{

            //}

            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}