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
    using DCCCommon;
    using DCCCommon.Conventions;
    using DCCCommon.Messages;
    using DCCDiscoveryService.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using EasySharp.NHelpers.CustomWrappers.Networking;
    using Interfaces;

    public class DiscoveryBasedCommunicationMediator : ICommunicationMediator
    {
        private bool _discoveryIsActive;

        private readonly int _discoveryResponsePort;
        private readonly ConcurrentBag<DiscoveryResponseMessage> _discoveryResponseMessages;
        private IPAddress _clientLocalIpAddress;

        //public IPAddress ClientLocalIpAddress { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }
        //public int ClientReceiveResponseTcpPort { get; set; }

        #region CONSTRUCTORS

        public DiscoveryBasedCommunicationMediator()
        {
            //_discoveryResponsePort = RNGUtil.Next(30_000, 60_000);
            _discoveryResponsePort = 36_456;
            _discoveryResponseMessages = new ConcurrentBag<DiscoveryResponseMessage>();
        }

        #endregion

        public string MakeRequest(RequestDataMessage requestMessage, int discoveryTimeout)
        {
            // Why not in constructor? Because preferred IP address can be changed be SO before MakeRequest() is called.
            _clientLocalIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();

            IPEndPoint mavenEndPoint = GetMavenEndPoint(discoveryTimeout);

            string data = RetrieveDataFromMaven(requestMessage, mavenEndPoint);

            return data;
        }

        #region Download Payload Data

        private string RetrieveDataFromMaven(
            RequestDataMessage requestMessage, IPEndPoint mavenEndPoint)
        {
            #region Trash

            //// Establish connection to the maven node
            //var tcpClient = new TcpClient();
            //await tcpClient.ConnectAsync(mavenEndPoint.Address, mavenEndPoint.Port).ConfigureAwait(false);
            //NetworkStream networkStream = tcpClient.GetStream();

            //// Prepare request message to be sent
            //string requestMessageXml = requestMessage.SerializeToXml();
            //byte[] dataToBeSent = requestMessageXml.ToUtf8EncodedByteArray();

            //#region Trash

            ////var streamReader = new StreamReader(networkStream, Encoding.UTF8);
            ////var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            ////await streamWriter.WriteAsync(requestMessageXml).ConfigureAwait(false);

            //#endregion

            //// Send request message
            //await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);

            //// Receive meta-data response
            //byte[] buffer = new byte[Common.BufferSize];
            //int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);
            //int payloadSize = BitConverter.ToInt32(buffer.Take(bytesRead).ToArray(), 0);

            //// Get Payload Data from maven
            //string data = await RetrieveDataPayloadFromMavenAsync(payloadSize, networkStream, buffer)
            //    .ConfigureAwait(false);

            //#region Trash

            ////streamReader.Close();
            ////streamWriter.Close();

            //#endregion

            //tcpClient.Close();

            //return data;

            #endregion

            var dataAgent = new DataAgent();

            // Retrieve data from the maven node
            string data = dataAgent.MakeRequest(requestMessage, mavenEndPoint, "SECRET");

            return data;
        }

        #endregion

        #region Maven Node Related

        private IPEndPoint GetMavenEndPoint(int discoveryTimeout)
        {
            // TIMEOUT WARNING $C$ T OBE REVIEWED ====================================================================

            // JoinMulticastGroup
            Socket mCastSocket = CreateMulticastSocket();

            // Run in background Discovery Response Listener Service
            //Task<LinkedList<DiscoveryResponseMessage>> getResponseMessagesTask = ReceiveDiscoveryResponseMessagesAsync();

            _discoveryIsActive = true;
            Thread discoveryListenerThread = new Thread(() => ReceiveDiscoveryResponseMessages(discoveryTimeout));
            discoveryListenerThread.Start();
            Thread.Sleep(500);

            // Discovery Init
            InitializeDiscoveryProcedure(mCastSocket);

            //await Task.Delay(timeoutMillisec).ConfigureAwait(false);
            //Thread.Sleep(timeoutMillisec);
            //ReceiveDiscoveryResponseMessages();

            //thread.Join(_timeoutMillisec);

            Thread.Sleep(TimeSpan.FromSeconds(discoveryTimeout));
            _discoveryIsActive = false; // Either info about all the nodes is collected or time is over.
            discoveryListenerThread.Abort();

            //thread.Abort();

            // Discovery Receive Response
            //LinkedList<DiscoveryResponseMessage> discoveryResponseMessages = await getResponseMessagesTask.ConfigureAwait(false);

            var discoveryResponseMessages = new List<DiscoveryResponseMessage>(_discoveryResponseMessages);

            Console.Out.WriteLine($"Total nodes discovered: {discoveryResponseMessages.Count}");

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
            #region Trash

            //var discoveryResponseMessages = await Task.Run(async () =>
            //{

            //}).ConfigureAwait(false);

            //return discoveryResponseMessages;

            //var responseHandlers = new ConcurrentBag<Task<DiscoveryResponseMessage>>();

            #endregion

            Thread thread = new Thread(() =>
            {
                Console.Out.WriteLine("Run the Receive Discovery Response SERVICE");
                IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
                Console.Out.WriteLine($"Listening to {IPAddress.Any}:{_discoveryResponsePort}");

                var tcpListener = new TcpListenerEx(IPAddress.Any, _discoveryResponsePort);

                try
                {
                    tcpListener.Start();
                    Console.Out.WriteLine($" [TCP] The socket is active? {tcpListener.Active}");
                    Console.WriteLine(" [TCP] The local End point is  :" + tcpListener.LocalEndpoint);
                    Console.WriteLine(" [TCP] Waiting for a connection.....\n");

                    TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(discoveryTimeout);
                    DateTime listeningStartTime = DateTime.Now;

                    #region Trash

                    //var timer = new Timer(state =>
                    //{
                    //    (state as TcpListenerEx)?.Stop();
                    //}, tcpListener,  1000*5, 1);

                    //async Task EnforceTcpListeningTimeout1()
                    //{
                    //    await Console.Out.WriteLineAsync("Start of async timer").ConfigureAwait(false);
                    //    await Task.Delay(TimeSpan.FromSeconds(timeoutSec)).ConfigureAwait(false);
                    //    tcpListener1.Stop();
                    //    await Console.Out.WriteLineAsync("Works just fine!").ConfigureAwait(false);
                    //    await Console.Out.WriteLineAsync("").ConfigureAwait(false);
                    //}

                    //Task.Run(EnforceTcpListeningTimeout1);

                    #endregion

                    // is serving continuously while timeout isn't reached
                    while (_discoveryIsActive
                           && DateTime.Now.Subtract(listeningStartTime) < timeoutTimeSpan)
                    {
                        Console.Out.WriteLine("Before accepting....");

                        Socket workerSoket = tcpListener.AcceptSocket();

                        #region Trash

                        //try
                        //{
                        //    tcpClient = tcpListener.AcceptTcpClient();
                        //}
                        //catch (Exception)
                        //{
                        //    await Console.Out.WriteLineAsync("Discovery procedure is terminated.")
                        //        .ConfigureAwait(false);
                        //    break;
                        //}

                        #endregion

                        Console.WriteLine($" [TCP] Connection accepted from: {{ {workerSoket.RemoteEndPoint} }}");
                        Console.WriteLine($" [TCP] SocketWorker is bound to: {{ {workerSoket.LocalEndPoint} }}");

                        #region Trash

                        //TcpServerWorker.Instance
                        //    .Init(workerTcpSocket, tcpListener)
                        //    .StartWorking();

                        //// TODO Unchecked modification
                        //if (tcpListener.Inactive)
                        //{
                        //    tcpListener.Stop();
                        //}

                        //Task<DiscoveryResponseMessage> responseHandlerTask1 = Task.Run(() =>
                        //{
                        //    return HandleResponseAsync(tcpListener, tcpClient);
                        //});

                        //responseHandlers.AddLast(responseHandlerTask1);

                        #endregion

                        new Thread(() =>
                        {
                            Console.Out.WriteLine(
                                $"[TCP] >> SERVER WORKER IS TALKING TO {workerSoket.RemoteEndPoint}");

                            //LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

                            if (tcpListener.Inactive)
                            {
                                Console.Out.WriteLine("[TCP] >> DISCOVERY LISTENER IS DOWN. Closing connection...");
                                return;
                            }

                            byte[] buffer = new byte[Common.UnicastBufferSize];

                            #region Trash

                            //int receivedBytes = workerTcpSocket.Receive(buffer);

                            //NetworkStream networkStream = workerSoket.GetStream();

                            //int receivedBytes = networkStream.Read(buffer, 0, buffer.Length);

                            #endregion

                            int receivedBytes = workerSoket.Receive(buffer);

                            #region Trash

                            //if (receivedBytes == 0)
                            //{
                            //    workerSoket.Close();

                            //    Console.Out.WriteLine(
                            //        @" [TCP]   >> DISCOVERY LISTENER WORKER says: ""No bytes received. Connection closed.""");

                            //    return;
                            //}

                            #endregion

                            workerSoket.Close();

                            #region Trash

                            //if (requestString == QuitServerCmd)
                            //{
                            //    connectionAlive = false;
                            //    serverMustStopServingRequests = true;

                            //    worker.Send("200 OK SHUTDOWN --res='TCP Server Halted'"
                            //        .ToFlowProtocolAsciiEncodedBytesArray());

                            //    Console.Out.WriteLine($" [TCP] Client closed connection");
                            //    Console.Out.WriteLine(" [TCP] Client turned off TCP server.");
                            //    continue;
                            //}
                            // SEND BACK A RESPONSE
                            //worker.Send(buffer);

                            #endregion

                            #region Trash

                            //if (serverMustStopServingRequests && _server.Active)
                            //{
                            //    _server.Stop();
                            //    Console.Out.WriteLine(" [TCP] SERVER HALTED");
                            //}

                            #endregion

                            //byte[] data = receivedBinaryData.SelectMany(batch => batch).ToArray();

                            byte[] data = buffer.Take(receivedBytes).ToArray();

                            string xmlData = data.ToUtf8String();

                            Console.Out.WriteLine(xmlData);

                            DiscoveryResponseMessage responseMessage = xmlData
                                .DeserializeTo<DiscoveryResponseMessage>();

                            // To be removed $C$
                            Console.Out.WriteLine(" [TCP]   >> DISCOVERY LISTENER has finished job");

                            _discoveryResponseMessages.Add(responseMessage);
                        }).Start();
                    }
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("[TCP] Grave error occured. Searver is dead.");
                    Console.Out.WriteLine($"e = {e.Message}");
                    Debug.WriteLine("[TCP] Grave error occured. Searver is dead.");
                    Debug.WriteLine($"e = {e.Message}");
                    Console.Out.WriteLine("[TCP] PRESS ANY KEY TO QUIT");
                    Console.ReadLine();

                    //throw; // TODO Unchecked modification
                    throw;
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

            #region Trash

            //foreach (Task<DiscoveryResponseMessage> handlerTask in responseHandlers)
            //{
            //    if (!handlerTask.IsCompleted)
            //    {
            //        // We do not care about the tasks that didn't get the job done
            //        continue;
            //    }
            //    DiscoveryResponseMessage discoveryResponseMessage = await handlerTask.ConfigureAwait(false);
            //    discoveryResponseMessages.AddLast(discoveryResponseMessage);
            //}

            #endregion
        }

        //private DiscoveryResponseMessage HandleResponseAsync(

        #region Trash

        //    TcpListenerEx tcpListener, TcpClient tcpWorker)
        //{
        //    Console.Out.WriteLine($"[TCP] >> SERVER WORKER IS TALKING TO { tcpWorker.Client.RemoteEndPoint}");

        //    LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

        //    while (true)
        //    {
        //        if (tcpListener.Inactive)
        //        {
        //            break;
        //        }

        //        byte[] buffer = new byte[Common.BufferSize];

        //        //int receivedBytes = workerTcpSocket.Receive(buffer);

        //        NetworkStream networkStream = tcpWorker.GetStream();

        //        int receivedBytes = networkStream.Read(buffer, 0, buffer.Length);

        //        if (receivedBytes == 0)
        //        {
        //            tcpWorker.Close();

        //            Console.Out.WriteLine(@" [TCP]   >> SERVER WORKER says: ""No bytes received. Connection closed.""");

        //            break;
        //        }

        //        receivedBinaryData.AddLast(buffer.Take(receivedBytes));

        //        #region Trash

        //        //if (requestString == QuitServerCmd)
        //        //{
        //        //    connectionAlive = false;
        //        //    serverMustStopServingRequests = true;

        //        //    worker.Send("200 OK SHUTDOWN --res='TCP Server Halted'"
        //        //        .ToFlowProtocolAsciiEncodedBytesArray());

        //        //    Console.Out.WriteLine($" [TCP] Client closed connection");
        //        //    Console.Out.WriteLine(" [TCP] Client turned off TCP server.");
        //        //    continue;
        //        //}
        //        // SEND BACK A RESPONSE
        //        //worker.Send(buffer);

        //        #endregion
        //    }

        //    tcpWorker.Close();

        //    #region Trash

        //    //if (serverMustStopServingRequests && _server.Active)
        //    //{
        //    //    _server.Stop();
        //    //    Console.Out.WriteLine(" [TCP] SERVER HALTED");
        //    //}

        //    #endregion

        //    byte[] data = receivedBinaryData.SelectMany(batch => batch).ToArray();

        //    string xmlData = data.ToUtf8String();

        //    DiscoveryResponseMessage responseMessage = xmlData.DeserializeTo<DiscoveryResponseMessage>();

        //    // To be removed $C$
        //    Console.Out.WriteLine(" [TCP]   >> SERVER WORKER finished job");
        //    return responseMessage;
        //}

        #endregion

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

            Console.Out.WriteLine($"Multicast socket is created for: {MulticastIPEndPoint.Address} & {_clientLocalIpAddress}");

            mCastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mCastOption);

            return mCastSocket;
        }

        #endregion
    }
}