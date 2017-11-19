namespace DCCCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using EasySharp.NHelpers.CustomExMethods;
    using Models;
    using static Conventions.Common;

    public class StartupConfigManager
    {
        private static readonly object PadLock = new object();

        private static readonly Lazy<StartupConfigManager> LazyInstance =
            new Lazy<StartupConfigManager>(() => new StartupConfigManager(), true);

        public static StartupConfigManager Default => LazyInstance.Value;
        //private const string ConfigFilePath = "StartupConfig.xml";

        protected string ConfigFilePath { get; set; }

        #region CONSTRUCTORS

        protected StartupConfigManager()
        {
            ConfigFilePath = GetStartupConfigPath();
        }

        #endregion

        private static string GetStartupConfigPath()
        {
            string executingPath = AppDomain.CurrentDomain.BaseDirectory;
            string startupConfigPath = Path.Combine(executingPath, StartupConfigFileName);
            return startupConfigPath;
        }

        #region Client Related

        public IEnumerable<NodeInfo> GetNodesIDsWithAdjacentNodesNo()
        {
            //var nodesIdRangPairList = new LinkedList<(int, int)>();
            var nodesIdRangPairList = new LinkedList<NodeInfo>();

            try
            {
                lock (PadLock)
                {
                    XElement root = XElement.Load(ConfigFilePath);

                    IEnumerable<XElement> nodes = root?.Elements(Node);

                    foreach (XElement node in nodes)
                    {
                        int id = node.Attribute(Id).GetAs<int>();

                        int remoteNodesNo = node.Descendants(RemoteNode).Count();

                        nodesIdRangPairList.AddLast(new NodeInfo
                        {
                            Id = id,
                            AdjacentNodesNo = remoteNodesNo
                        });
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return nodesIdRangPairList;
        }

        public IEnumerable<IPEndPoint> GetProxyConnectedNodesEndPoints()
        {
            LinkedList<IPEndPoint> connectedNodesEndPoints = new LinkedList<IPEndPoint>();

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                foreach (XElement node in root.Descendants(Node))
                {
                    try
                    {
                        string nodeIpAddressString = node.Element(LocalIpAddress).Value;

                        IPAddress nodeIpAddress = IPAddress.Parse(nodeIpAddressString);
                        int nodeTcpServingPort = node.Element(TcpServingPort).GetAs<int>(-1);

                        var nodeEndPoint = new IPEndPoint(nodeIpAddress, nodeTcpServingPort);

                        connectedNodesEndPoints.AddLast(nodeEndPoint);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return connectedNodesEndPoints;
        }

        public int GetDiscoveryClientResponseTcpPort()
        {
            string responseTcpPortString;

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                responseTcpPortString = root.Element(Client)
                                            ?.Element(Discovery)
                                            ?.Element(ResponseTcpPort)
                                            ?.Value
                                        ?? string.Empty;
            }

            int tcpResponsePort;

            try
            {
                tcpResponsePort = int.Parse(responseTcpPortString);
            }
            catch (Exception)
            {
                tcpResponsePort = -1;
            }

            return tcpResponsePort;
        }

        public IPEndPoint GetDiscoveryClientMulticastIPEndPoint()
        {
            string multicastIpAddressString = string.Empty;
            string portString = string.Empty;

            try
            {
                lock (PadLock)
                {
                    XElement root = XElement.Load(ConfigFilePath);

                    multicastIpAddressString = root.Element(Client)
                                                   ?.Element(Discovery)
                                                   ?.Element(MulticastIpAddress)
                                                   ?.Value
                                               ?? string.Empty;

                    portString = root.Element(Client)
                                     ?.Element(Discovery)
                                     ?.Element(MulticastPort)
                                     ?.Value
                                 ?? string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            IPEndPoint multicastIpEndPoint = default;

            try
            {
                multicastIpEndPoint = new IPEndPoint(
                    IPAddress.Parse(multicastIpAddressString),
                    int.Parse(portString));
            }
            catch
            {
                // ignored
            }

            return multicastIpEndPoint;
        }

        public IPEndPoint GetProxyEndPoint()
        {
            string ipAddressString = string.Empty;
            string tcpPortString = string.Empty;

            try
            {
                lock (PadLock)
                {
                    XElement root = XElement.Load(ConfigFilePath);

                    ipAddressString = root.Element(Client)
                                          ?.Element(Proxy)
                                          ?.Element(RemoteIpAddress)
                                          ?.Value
                                      ?? string.Empty;

                    tcpPortString = root.Element(Client)
                                        ?.Element(Proxy)
                                        ?.Element(RemotePort)
                                        ?.Value
                                    ?? string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            IPEndPoint ipEndPoint = default;

            try
            {
                ipEndPoint = new IPEndPoint(
                    IPAddress.Parse(ipAddressString),
                    int.Parse(tcpPortString));
            }
            catch
            {
                // ignored
            }

            return ipEndPoint;
        }

        public bool ExistsKey(string key)
        {
            bool exists = false;

            lock (PadLock)
            {
                try
                {
                    XElement root = XElement.Load(ConfigFilePath);
                    exists = root.Descendants(key).Any();
                }
                catch
                {
                    // ignored
                }
            }

            return exists;
        }

        #endregion

        #region Node Related

        public string GetNodeDataSourcePath(int nodeId)
        {
            string dataSourcePath = string.Empty;

            try
            {
                XElement node = GetNodeById(nodeId);

                string dataSourceFileName = string.Empty;

                lock (PadLock)
                {
                    dataSourceFileName = node?.Element(DataSource)?.Value ?? string.Empty;
                }

                string executingPath = AppDomain.CurrentDomain.BaseDirectory;
                dataSourcePath = Path.Combine(executingPath, dataSourceFileName);
            }
            catch (Exception)
            {
                // ignored
            }

            return dataSourcePath;
        }

        public int GetNodeTcpServingPort(int nodeId)
        {
            string tcpServingPortString = string.Empty;

            try
            {
                XElement node = GetNodeById(nodeId);

                lock (PadLock)
                {
                    tcpServingPortString = node?.Element(TcpServingPort)?.Value
                                           ?? string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            int tcpServingPort;

            try
            {
                tcpServingPort = int.Parse(tcpServingPortString);
            }
            catch (Exception)
            {
                tcpServingPort = -1;
            }

            return tcpServingPort;
        }

        public IEnumerable<(int, IPEndPoint)> GetAdjacentNodesEndPointsWithIDs(int nodeId)
        {
            var remoteNodesEndPoints = new LinkedList<(int, IPEndPoint)>();

            try
            {
                IEnumerable<XElement> adjacentNodes = GetAdjacentNodes(nodeId);

                lock (PadLock)
                {
                    foreach (XElement adjacentNode in adjacentNodes)
                    {
                        string remoteIpAddressString = adjacentNode?.Element(RemoteIpAddress)?.Value
                                                       ?? string.Empty;

                        string remotePortString = adjacentNode?.Element(RemotePort)?.Value
                                                  ?? string.Empty;

                        string adjacentNodeId = adjacentNode?.Attribute(Id)?.Value
                                                ?? string.Empty;

                        try
                        {
                            var ipEndPoint = new IPEndPoint(
                                IPAddress.Parse(remoteIpAddressString),
                                int.Parse(remotePortString));

                            int id = int.Parse(adjacentNodeId);

                            remoteNodesEndPoints.AddLast((id, ipEndPoint));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return remoteNodesEndPoints;
        }

        public IPEndPoint GetNodeMulticastIPEndPoint(int nodeId)
        {
            string multicastIpAddressString = string.Empty;
            string portString = string.Empty;

            try
            {
                XElement node = GetNodeById(nodeId);

                lock (PadLock)
                {
                    multicastIpAddressString = node?.Element(MulticastIpAddress)?.Value
                                               ?? string.Empty;

                    portString = node?.Element(MulticastPort)?.Value
                                 ?? string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            IPEndPoint multicastIpEndPoint = default;

            try
            {
                multicastIpEndPoint = new IPEndPoint(
                    IPAddress.Parse(multicastIpAddressString),
                    int.Parse(portString));
            }
            catch
            {
                // ignored
            }

            return multicastIpEndPoint;
        }

        private IEnumerable<XElement> GetAdjacentNodes(int nodeId)
        {
            IEnumerable<XElement> adjacentNodes;

            XElement node = GetNodeById(nodeId);

            lock (PadLock)
            {
                adjacentNodes = node?.Element(AdjacentNodes)?.Descendants(RemoteNode);
            }

            return adjacentNodes;
        }

        private XElement GetNodeById(int nodeId)
        {
            XElement node;

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                node = root?.Descendants(Node)
                    ?.FirstOrDefault(elem => nodeId.ToString().Equals(elem.Attribute(Id)?.Value));
            }

            return node;
        }

        public IPEndPoint GetNodeIPEndPointById(int nodeId)
        {
            XElement node = GetNodeById(nodeId);

            string nodeIpAddressString = string.Empty;

            lock (PadLock)
            {
                nodeIpAddressString = node?.Element(LocalIpAddress)?.Value
                                      ?? string.Empty;
            }

            int servingPort = GetNodeTcpServingPort(nodeId);

            var nodeEndPoint = new IPEndPoint(
                IPAddress.Parse(nodeIpAddressString),
                servingPort);

            return nodeEndPoint;
        }

        #endregion
    }
}