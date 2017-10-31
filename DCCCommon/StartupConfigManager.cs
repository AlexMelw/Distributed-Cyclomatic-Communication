namespace DCCCommon
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
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

        public IPAddress GetLocalIpAddress(string consumer, string searchKey)
        {
            string localIpAddressString;

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                localIpAddressString = root.Descendants(consumer)
                                           .FirstOrDefault()
                                           ?.Element(searchKey)
                                           ?.Value
                                       ?? string.Empty;
            }

            IPAddress.TryParse(localIpAddressString, out var localIpAddress);

            return localIpAddress;
        }

        /// <summary>
        ///     Is used by Node Worker
        /// </summary>
        public IPAddress GetLocalIpAddress(string node, string localIpAddress, int id)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint GetMulticastIPEndPoint(string consumer)
        {
            string multicastIpAddressString;
            string portString;

            lock (PadLock)
            {
                XElement root = XElement.Load(ConfigFilePath);

                multicastIpAddressString = root.Descendants(consumer)
                                               .FirstOrDefault()
                                               ?.Element(MulticastIpAddress)
                                               ?.Value
                                           ?? string.Empty;

                portString = root.Descendants(consumer)
                                 .FirstOrDefault()
                                 ?.Element(MulticastPort)
                                 ?.Value
                             ?? string.Empty;
            }

            IPEndPoint multicastIPEndPoint = null;

            try
            {
                multicastIPEndPoint = new IPEndPoint(
                    IPAddress.Parse(multicastIpAddressString),
                    int.Parse(portString));
            }
            catch
            {
                // ignored
            }

            return multicastIPEndPoint;
        }

        /// <summary>
        ///     Is used by Node Worker
        /// </summary>
        public IPEndPoint GetMulticastIPEndPoint(string consumer, int id)
        {
            throw new NotImplementedException();
        }
    }
}