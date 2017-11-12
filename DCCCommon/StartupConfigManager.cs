namespace DCCCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using static Conventions.Common;

    public class StartupConfigManager
    {
        private static readonly object PadLock = new object();

        private static readonly Lazy<StartupConfigManager> LazyInstance =
            new Lazy<StartupConfigManager>(() => new StartupConfigManager(), true);
        //private const string ConfigFilePath = "StartupConfig.xml";

        public string ConfigFilePath { get; set; }

        public static StartupConfigManager Default => LazyInstance.Value;

        #region CONSTRUCTORS

        private StartupConfigManager()
        {
            var startupConfigPath = GetStartupConfigPath();
            ConfigFilePath = startupConfigPath;
        }

        #endregion

        private static string GetStartupConfigPath()
        {
            string executingPath = AppDomain.CurrentDomain.BaseDirectory;
            string startupConfigPath = Path.Combine(executingPath, StartupConfigFileName);
            return startupConfigPath;
        }

        #region Client Related

        public IPAddress GetClientLocalIpAddress()
        {
            string localIpAddressString;

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                localIpAddressString = root.Element(Client)
                                           ?.Element(LocalIpAddress)
                                           ?.Value
                                       ?? string.Empty;
            }

            IPAddress.TryParse(localIpAddressString, out IPAddress localIpAddress);

            return localIpAddress;
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

        public IPAddress GetNodeLocalIpAddress(int nodeId)
        {
            string localIpAddressString = string.Empty;

            try
            {
                XElement node = GetNodeById(nodeId);

                lock (PadLock)
                {
                    localIpAddressString = node?.Element(LocalIpAddress)?.Value
                                           ?? string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            IPAddress.TryParse(localIpAddressString, out IPAddress localIpAddress);

            return localIpAddress;
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

        public IEnumerable<IPEndPoint> GetAdjacentNodesEndPoints(int nodeId)
        {
            var remoteNodesEndPoints = new LinkedList<IPEndPoint>();

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

                        try
                        {
                            var ipEndPoint = new IPEndPoint(
                                IPAddress.Parse(remoteIpAddressString),
                                int.Parse(remotePortString));

                            remoteNodesEndPoints.AddLast(ipEndPoint);
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

        #endregion
    }
}