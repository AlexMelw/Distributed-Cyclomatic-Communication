namespace DCCCommon.Conventions
{
    using System.Xml.Linq;

    public static class Common
    {
        public const string DataSource = "data-source";
        public const int UnicastBufferSize = 8192;
        public const int MulticastBufferSize = 8192;

        #region ConfigName

        public const string StartupConfigFileName = "StartupConfig.xml";

        #endregion

        #region Client

        public const string Client = "client";

        #endregion

        #region Proxy

        public const string Proxy = "proxy";

        #endregion

        #region Discovery

        public const string Discovery = "discovery";
        public const string ResponseTcpPort = "response-tcpPort";
        public const string MulticastIpAddress = "multicast-ipAddress";
        public const string MulticastPort = "multicast-port";

        #endregion

        #region Common

        public const string LocalIpAddress = "local-ipAddress";
        public const string RemotePort = "remote-tcpPort";
        public const string RemoteIpAddress = "remote-ipAddress";

        #endregion

        #region Node

        public const string Id = "id";
        public const string Node = "node";
        public const string TcpServingPort = "tcpserving-port";
        public const string AdjacentNodes = "adjacent-nodes";
        public const string RemoteNode = "remote-node";

        #endregion

        #region Data Format

        public const string Xml = "XML";
        public const string Json = "JSON";

        #endregion
    }
}