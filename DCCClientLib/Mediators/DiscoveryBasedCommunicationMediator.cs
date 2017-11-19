namespace DCCClientLib.Mediators
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using DCCCommon.Agents;
    using DCCCommon.Conventions;
    using DCCCommon.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using EasySharp.NHelpers.CustomWrappers.Networking;
    using EasySharp.NHelpers.Utils.Cryptography;
    using Interfaces;

    public class DiscoveryBasedCommunicationMediator : ICommunicationMediator
    {
        private bool _discoveryIsActive;
        private readonly int _discoveryResponsePort;
        private readonly ConcurrentBag<DiscoveryResponseMessage> _discoveryResponseMessages;
        private IPAddress _clientLocalIpAddress;

        public IPEndPoint MulticastIPEndPoint { get; set; }

        #region CONSTRUCTORS

        public DiscoveryBasedCommunicationMediator()
        {
            _discoveryResponsePort = RNGUtil.Next(30_001, 60_000);
            _discoveryResponseMessages = new ConcurrentBag<DiscoveryResponseMessage>();
        }

        #endregion

        public string MakeRequest(RequestDataMessage requestMessage, int discoveryTimeout)
        {
            // Why not in constructor? Because preferred IP address can be changed be SO before MakeRequest() is called.
            _clientLocalIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();

            IPEndPoint mavenEndPoint = GetMavenEndPoint(discoveryTimeout);

            // Retrieve data from the maven node
            string data = new DataAgent().MakeRequest(requestMessage, mavenEndPoint, "SECRET");

            return data;
        }

        #region Maven Node Related

        private IPEndPoint GetMavenEndPoint(int discoveryTimeout)
        {
            // TIMEOUT WARNING $C$ T OBE REVIEWED ====================================================================

            // JoinMulticastGroup
            Socket mCastSocket = CreateMulticastSocket();

            // Run in background Discovery Response Listener Service
            _discoveryIsActive = true;
            Thread discoveryListenerThread = new Thread(() => ReceiveDiscoveryResponseMessages(discoveryTimeout));
            discoveryListenerThread.Start();
            Thread.Sleep(500);

            // Discovery Init
            InitializeDiscoveryProcedure(mCastSocket);

            Thread.Sleep(TimeSpan.FromSeconds(discoveryTimeout));
            _discoveryIsActive = false; // Either info about all the nodes is collected or time is over.
            discoveryListenerThread.Abort();

            // Discovery Receive Response
            var discoveryResponseMessages = new List<DiscoveryResponseMessage>(_discoveryResponseMessages);

            string nL = Environment.NewLine;
            Console.Out.WriteLine($"Total nodes discovered: {discoveryResponseMessages.Count}{nL}");

            // Discovery Process Response Results
            IPEndPoint mavenEndPoint = IdentifyMavenNode(discoveryResponseMessages);

            return mavenEndPoint;
        }

        private IPEndPoint IdentifyMavenNode(List<DiscoveryResponseMessage> discoveryResponseMessages)
        {
            int maxConnectedNodes = discoveryResponseMessages.Count == 0
                ? 0
                : discoveryResponseMessages.Max(m => m.NodeConnectionNum);

            DiscoveryResponseMessage maven = discoveryResponseMessages.FirstOrDefault(message =>
                message.NodeConnectionNum == maxConnectedNodes);

            //await Console.Out.WriteLineAsync("Before maven null").ConfigureAwait(false);

            if (maven == null)
            {
                //await Console.Out.WriteLineAsync("Maven is NULL").ConfigureAwait(false);
                Console.Out.WriteLine("No nodes were discovered! Exiting application...");
                Environment.Exit(1);
            }

            IPAddress mavenIpAddress = IPAddress.Parse(maven.IPAddress);
            IPEndPoint mavenEndPoint = new IPEndPoint(mavenIpAddress, maven.ListeningPort);

            return mavenEndPoint;
        }

        private void ReceiveDiscoveryResponseMessages(int discoveryTimeout)
        {
            Thread thread = new Thread(() =>
            {
                //Console.Out.WriteLine("Run the Receive Discovery Response SERVICE");
                Console.Out.WriteLine($"DISCOVERY SERVICE is running [ timeout: {discoveryTimeout} sec. ]");
                IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
#if DEBUG
                Console.Out.WriteLine($"Listening to {IPAddress.Any}:{_discoveryResponsePort}");
#endif

                var tcpListener = new TcpListenerEx(IPAddress.Any, _discoveryResponsePort);

                try
                {
                    tcpListener.Start();
#if DEBUG
                    Console.Out.WriteLine($" [TCP] The socket is active? {tcpListener.Active}");
                    Console.WriteLine(" [TCP] The local End point is  :" + tcpListener.LocalEndpoint);
                    Console.WriteLine(" [TCP] Waiting for a connection.....\n");
#endif

                    TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(discoveryTimeout);
                    DateTime listeningStartTime = DateTime.Now;

                    // is serving continuously while timeout isn't reached
                    while (_discoveryIsActive
                           && DateTime.Now.Subtract(listeningStartTime) < timeoutTimeSpan)
                    {
#if DEBUG
                        Console.Out.WriteLine("Before accepting....");
#endif

                        Socket workerSoket = tcpListener.AcceptSocket();

#if DEBUG
                        Console.WriteLine($" [TCP] Connection accepted from: {{ {workerSoket.RemoteEndPoint} }}");
                        Console.WriteLine($" [TCP] SocketWorker is bound to: {{ {workerSoket.LocalEndPoint} }}");
#endif
                        new Thread(() =>
                        {
#if DEBUG
                            Console.Out.WriteLine(
                                $"[TCP] >> SERVER WORKER IS TALKING TO {workerSoket.RemoteEndPoint}");
#endif

                            if (tcpListener.Inactive)
                            {
#if DEBUG
                                Console.Out.WriteLine("[TCP] >> DISCOVERY LISTENER IS DOWN. Closing connection...");

#endif
                                return;
                            }

                            byte[] buffer = new byte[Common.UnicastBufferSize];

                            int receivedBytes = workerSoket.Receive(buffer);

                            workerSoket.Close();

                            byte[] data = buffer.Take(receivedBytes).ToArray();

                            string xmlData = data.ToUtf8String();

#if DEBUG
                            Console.Out.WriteLine(xmlData);
#endif

                            DiscoveryResponseMessage responseMessage = xmlData
                                .DeserializeTo<DiscoveryResponseMessage>();

#if DEBUG
                            Console.Out.WriteLine(" [TCP]   >> DISCOVERY LISTENER has finished job");
#endif

                            _discoveryResponseMessages.Add(responseMessage);
                        }).Start();
                    }
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("DISCOVERY SERVICE has crashed.");
                }
                finally
                {
                    if (tcpListener.Active)
                    {
                        tcpListener.Stop();
                    }
                }
            });

            thread.Start();
        }

        private void InitializeDiscoveryProcedure(Socket mCastSocket)
        {
            IPAddress localIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();


            var discoveryRequestMessage = new MulticastDiscoveryRequestMessage
            {
                IPAddress = localIpAddress.ToString(),
                ListeningPort = _discoveryResponsePort
            };

            // Send multicast packets to the listener.
            IPEndPoint remoteEndPoint = new IPEndPoint(MulticastIPEndPoint.Address, MulticastIPEndPoint.Port);

            string xml = discoveryRequestMessage.SerializeToXml();
            byte[] dataToBeSent = xml.ToUtf8EncodedByteArray();

            mCastSocket.SendTo(dataToBeSent, remoteEndPoint);

            Console.WriteLine("Initializing discovery procedure...");

            mCastSocket.Close();
        }

        private Socket CreateMulticastSocket()
        {
            var mCastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var localEP = new IPEndPoint(_clientLocalIpAddress, 0);

            // Bind this endpoint to the multicast socket.
            mCastSocket.Bind(localEP); // bind socket to 127.0.0.1:*

            // Define a MulticastOption object specifying the multicast group address and the local IP address.
            // The multicast group address is the same as the address used by the listener.
            var mCastOption = new MulticastOption(MulticastIPEndPoint.Address, _clientLocalIpAddress);

#if DEBUG
            Console.Out.WriteLine(
                $"Multicast socket is created for: {MulticastIPEndPoint.Address} & {_clientLocalIpAddress}");
#endif
            mCastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mCastOption);

            return mCastSocket;
        }

        #endregion
    }
}