namespace DCCCommon.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using Conventions;
    using NUnit.Framework;

    [TestFixture]
    public class StartupConfigManagerTests
    {
        private static string GetStartupConfigPath()
        {
            string executingPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executingPath, "StartupConfigTest.xml");
            return filePath;
        }

        #region Node Related Tests

        [TestCase(1)]
        public void GetNodeLocalIpAddressTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testIpAddress = IPAddress.Parse("127.0.0.1");

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IPAddress nodeLocalIpAddress = StartupConfigManager.Default
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

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            int tcpServingPort = StartupConfigManager.Default
                .GetNodeTcpServingPort(nodeId);

            // Assert
            Assert.That(tcpServingPort, Is.EqualTo(testTcpServingPort));
        }

        [TestCase(1)]
        public void GetNodeMulticastIPEndPointTest(int nodeId)
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testIpEndPoint = new IPEndPoint(IPAddress.Parse("224.1.2.3"), 27549);

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint nodeMulticastIpEndPoint = StartupConfigManager.Default
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

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IEnumerable<IPEndPoint> adjacentNodesEndPoints = StartupConfigManager.Default
                .GetAdjacentNodesEndPoints(nodeId);

            // Assert
            Assert.That(adjacentNodesEndPoints, Is.EquivalentTo(testAdjacentNodesEndPoints));
        }

        #endregion


        [Test]
        public void GetClientLocalIpAddressTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            var testClientLocalIpAddress = IPAddress.Parse("127.0.0.1");

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IPAddress clientLocalIpAddress = StartupConfigManager.Default
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

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            int discoveryClientResponseTcpPort = StartupConfigManager.Default
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
                new IPEndPoint(IPAddress.Parse("224.1.2.3"), 27549);

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint discoveryClientMulticastIpEndPoint = StartupConfigManager.Default
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

            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            IPEndPoint proxyEndPoint = StartupConfigManager.Default
                .GetProxyEndPoint();

            // Assert
            Assert.That(proxyEndPoint, Is.EqualTo(testProxyIpEndPoint));
        }

        [Test]
        public void ExistsKeyTest()
        {
            // Arrange
            string filePath = GetStartupConfigPath();
            StartupConfigManager.Default.ConfigFilePath = filePath;

            // Act
            bool existsKeyDiscovery = StartupConfigManager.Default
                .ExistsKey(Common.Discovery);

            bool existsKeyProxy = StartupConfigManager.Default
                .ExistsKey(Common.Proxy);

            Console.Out.WriteLine("existsKeyDiscovery = {0}", existsKeyDiscovery);
            Console.Out.WriteLine("existsKeyProxy = {0}", existsKeyProxy);

            // Assert
            Assert.That(new[] { existsKeyProxy, existsKeyDiscovery }, Has.Some.EqualTo(true));
        }
    }
}