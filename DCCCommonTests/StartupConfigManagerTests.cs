namespace DCCCommon.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using DCCCommon;
    using DCCCommon.Conventions;
    using NUnit.Framework;

    [TestFixture]
    public class StartupConfigManagerTestExTests
    {
        private static string GetStartupConfigPath()
        {
            string executingPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executingPath, "StartupConfigTest.xml");
            return filePath;
        }

        #region Node Related Tests

        [TestCase(1)]
        public void GetNodeDataSourcePathTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            string testDataSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Employees.xml"); ;

            Console.Out.WriteLine("testDataSourcePath = {0}", testDataSourcePath);

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            string dataSourcePath = StartupConfigManagerTestEx.Default
                .GetNodeDataSourcePath(nodeId);

            Console.Out.WriteLine("dataSourcePath = {0}", dataSourcePath);

            // Assert
            Assert.That(dataSourcePath, Is.EqualTo(testDataSourcePath));
        }

        [TestCase(1)]
        public void GetNodeLocalIpAddressTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testIpAddress = IPAddress.Parse("127.0.0.1");

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IPAddress nodeLocalIpAddress = StartupConfigManagerTestEx.Default
                .GetNodeLocalIpAddress(nodeId);

            // Assert
            Assert.That(nodeLocalIpAddress, Is.EqualTo(testIpAddress));
        }

        [TestCase(1)]
        public void GetTcpServingPortTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            int testTcpServingPort = 28001;

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            int tcpServingPort = StartupConfigManagerTestEx.Default
                .GetNodeTcpServingPort(nodeId);

            // Assert
            Assert.That(tcpServingPort, Is.EqualTo(testTcpServingPort));
        }

        [TestCase(1)]
        public void GetNodeMulticastIPEndPointTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testIpEndPoint = new IPEndPoint(IPAddress.Parse("228.1.2.3"), 27549);

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint nodeMulticastIpEndPoint = StartupConfigManagerTestEx.Default
                .GetNodeMulticastIPEndPoint(nodeId);

            // Assert
            Assert.That(nodeMulticastIpEndPoint, Is.EqualTo(testIpEndPoint));
        }

        [TestCase(1)]
        public void GetAdjacentNodesEndPointsTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testAdjacentNodesEndPoints = new List<IPEndPoint>
            {
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28005),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28002)
            };

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IEnumerable<IPEndPoint> adjacentNodesEndPoints = StartupConfigManagerTestEx.Default
                .GetAdjacentNodesEndPoints(nodeId);

            // Assert
            Assert.That(adjacentNodesEndPoints, Is.EquivalentTo(testAdjacentNodesEndPoints));
        }

        #endregion


        #region Client Related Tests

        [Test]
        public void GetClientLocalIpAddressTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testClientLocalIpAddress = IPAddress.Parse("127.0.0.1");

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IPAddress clientLocalIpAddress = StartupConfigManagerTestEx.Default
                .GetClientLocalIpAddress();

            // Assert
            Assert.That(clientLocalIpAddress, Is.EqualTo(testClientLocalIpAddress));
        }

        [Test]
        public void GetDiscoveryClientResponseTcpPortTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            int testDiscoveryResponseTcpPort = 27001;

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            int discoveryClientResponseTcpPort = StartupConfigManagerTestEx.Default
                .GetDiscoveryClientResponseTcpPort();

            // Assert
            Assert.That(discoveryClientResponseTcpPort, Is.EqualTo(testDiscoveryResponseTcpPort));
        }

        [Test]
        public void GetDiscoveryClientMulticastIPEndPointTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            IPEndPoint testDiscoveryClientMulticastIpEndPoint =
                new IPEndPoint(IPAddress.Parse("228.1.2.3"), 27549);

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint discoveryClientMulticastIpEndPoint = StartupConfigManagerTestEx.Default
                .GetDiscoveryClientMulticastIPEndPoint();

            // Assert
            Assert.That(discoveryClientMulticastIpEndPoint, Is.EqualTo(testDiscoveryClientMulticastIpEndPoint));
        }

        [Test]
        public void GetProxyEndPointTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testProxyIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);

            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint proxyEndPoint = StartupConfigManagerTestEx.Default
                .GetProxyEndPoint();

            // Assert
            Assert.That(proxyEndPoint, Is.EqualTo(testProxyIpEndPoint));
        }

        [Test]
        public void ExistsKeyTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            StartupConfigManagerTestEx.Default.ConfigFilePath = filePath;

            // Act
            bool existsKeyDiscovery = StartupConfigManagerTestEx.Default
                .ExistsKey(Common.Discovery);

            bool existsKeyProxy = StartupConfigManagerTestEx.Default
                .ExistsKey(Common.Proxy);

            Console.Out.WriteLine("existsKeyDiscovery = {0}", existsKeyDiscovery);
            Console.Out.WriteLine("existsKeyProxy = {0}", existsKeyProxy);

            // Assert
            Assert.That(new[] { existsKeyProxy, existsKeyDiscovery }, Has.Some.EqualTo(true));
        }

        [Test]
        public void GetProxyConnectedNodesEndPointsTest()
        {
            // Arrange
            string startupConfigPath = GetStartupConfigPath();
            StartupConfigManagerTestEx.Default.ConfigFilePath = startupConfigPath;
            List<IPEndPoint> testConnectedNodesEndPoints = new List<IPEndPoint>
            {
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28006),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28005),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28004),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28003),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28002),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28001),
            };

            // Act
            IEnumerable<IPEndPoint> proxyConnectedNodesEndPoints = StartupConfigManagerTestEx.Default
                .GetProxyConnectedNodesEndPoints();

            // Assert
            Assert.That(proxyConnectedNodesEndPoints, Is.EquivalentTo(testConnectedNodesEndPoints));
        }

        #endregion
    }

    class StartupConfigManagerTestEx : StartupConfigManager
    {
        private static readonly Lazy<StartupConfigManagerTestEx> LazyInstance =
            new Lazy<StartupConfigManagerTestEx>(() => new StartupConfigManagerTestEx(), true);

        public new static StartupConfigManagerTestEx Default => LazyInstance.Value;

        private StartupConfigManagerTestEx() { }

        public new string ConfigFilePath
        {
            get => base.ConfigFilePath;
            set => base.ConfigFilePath = value;
        }
    }
}