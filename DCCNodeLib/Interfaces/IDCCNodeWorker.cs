namespace DCCNodeLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCNodeWorker : IDisposable
    {
        IPAddress LocalIpAddress { get; set; }
        IPEndPoint MulticastIPEndPoint { get; set; }
        int TcpServingPort { get; set; }
        IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }
        Task StartAsync();
        Task InitAsync(int nodeId);
    }
}