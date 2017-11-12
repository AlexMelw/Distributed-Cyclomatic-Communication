namespace DCCDiscoveryService
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Interfaces;

    public class DiscoveryService : IDiscoveryService
    {
        public DiscoveryService() { }

        public Task<IPEndPoint> GetMavenEndPointAsync() { }

        public void Dispose() { }
    }
}