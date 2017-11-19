namespace DCCNodeLib.Interfaces
{
    using System.Collections.Generic;
    using System.Net;

    public interface IDCCNodeWorker
    {
        int CurrentNodeId { get; set; }
        string DataSourcePath { get; set; }
        IPEndPoint MulticastIPEndPoint { get; set; }
        IEnumerable<(int, IPEndPoint)> AdjacentNodesEndPointsWithIDs { get; set; }
        int TcpServingPort { get; set; }
        void Start();
        void Init(int nodeId);
    }
}