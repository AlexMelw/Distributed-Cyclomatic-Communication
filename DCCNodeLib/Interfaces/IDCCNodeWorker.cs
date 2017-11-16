namespace DCCNodeLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCNodeWorker
    {
        int CurrentNodeId { get; set; }
        string DataSourcePath { get; set; }
        IPAddress LocalIpAddress { get; set; }
        IPEndPoint MulticastIPEndPoint { get; set; }
        int TcpServingPort { get; set; }
        IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }
        void Start();
        void Init(int nodeId);
    }
}