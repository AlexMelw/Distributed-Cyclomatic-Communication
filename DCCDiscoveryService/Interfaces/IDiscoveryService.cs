namespace DCCDiscoveryService.Interfaces
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDiscoveryService : IDisposable
    {
        Task<IPEndPoint> GetMavenEndPointAsync();
    }
}