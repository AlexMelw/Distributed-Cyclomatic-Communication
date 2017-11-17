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
        IPEndPoint MulticastIPEndPoint { get; set; }
        IEnumerable<(int, IPEndPoint)> AdjacentNodesEndPointsWithIDs { get; set; }
        int TcpServingPort { get; set; }
        void Start();
        void Init(int nodeId);

        //IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }
        //IPAddress LocalIpAddress { get; set; }
    }
}