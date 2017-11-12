namespace DCCCommon
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Conventions;
    using static Conventions.Common;

    public class StartupConfigManager
    {
        private const string ConfigFilePath = "StartupConfig.xml";

        private static readonly object PadLock = new object();

        private static readonly Lazy<StartupConfigManager> LazyInstance =
            new Lazy<StartupConfigManager>(() => new StartupConfigManager(), true);

        public static StartupConfigManager Default => LazyInstance.Value;

        #region CONSTRUCTORS

        protected StartupConfigManager() { }

        #endregion

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
                                        ?.Element(ResponseTcpPort)
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
                lock (PadLock)
                {
                    XElement root = XElement.Load(ConfigFilePath);

                    XElement node = GetNodeById(nodeId);

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

        public int GetTcpServingPort(int nodeId)
        {
            string tcpServingPortString = string.Empty;

            try
            {
                lock (PadLock)
                {
                    XElement node = GetNodeById(nodeId);

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

        private static XElement GetNodeById(int nodeId)
        {
            XElement root = XElement.Load(ConfigFilePath);

            XElement node = root.Descendants(Node)
                ?.FirstOrDefault(elem => nodeId.ToString().Equals(elem.Attribute(Id)?.Value));

            return node;
        }

        public IPEndPoint GetNodeMulticastIPEndPoint(int nodeId)
        {
            string multicastIpAddressString = string.Empty;
            string portString = string.Empty;

            try
            {
                lock (PadLock)
                {
                    XElement node = GetNodeById(nodeId);

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

        #endregion
    }
}