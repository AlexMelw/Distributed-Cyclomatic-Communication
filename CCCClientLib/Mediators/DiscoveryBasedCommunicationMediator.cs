namespace DCCClientLib.Mediators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Conventions;
    using DCCCommon.Messages;
    using DCCDiscoveryService.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using EasySharp.NHelpers.CustomWrappers.Networking;
    using EasySharp.NHelpers.Utils.Cryptography;
    using Interfaces;

    public class DiscoveryBasedCommunicationMediator : ICommunicationMediator
    {
        private readonly int _discoveryResponsePort;
        public IPAddress ClientLocalIpAddress { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }

        #region CONSTRUCTORS

        public DiscoveryBasedCommunicationMediator()
        {
            //_discoveryResponsePort = RNGUtil.Next(30_000, 60_000);
            _discoveryResponsePort = 36_456;
        }

        #endregion

        public async Task<string> MakeRequestAsync(RequestDataMessage requestMessage)
        {
            IPEndPoint mavenEndPoint = await GetMavenEndPointAsync().ConfigureAwait(false);

            string data = await RetrieveDataFromMavenAsync(requestMessage, mavenEndPoint).ConfigureAwait(false);

            return data;
        }

        #region Maven Node Related

        private async Task<IPEndPoint> GetMavenEndPointAsync()
        {
            // JoinMulticastGroup
            var mCastSocket = await CreateMulticastSocketAsync().ConfigureAwait(false);

            // Run in background Discovery Response Listener Service
            Task<LinkedList<DiscoveryResponseMessage>>
                getResponseMessagesTask = ReceiveDiscoveryResponseMessagesAsync();

            // Discovery Init
            await InitializeDiscoveryProcedureAsync(mCastSocket).ConfigureAwait(false);

            // Discovery Receive Response
            var discoveryResponseMessages = await getResponseMessagesTask.ConfigureAwait(false);

            // Discovery Process Response Results
            var mavenEndPoint = await IdentifyMavenNodeAsync(discoveryResponseMessages).ConfigureAwait(false);

            return mavenEndPoint;
        }

        private async Task<IPEndPoint> IdentifyMavenNodeAsync(
            LinkedList<DiscoveryResponseMessage> discoveryResponseMessages)
        {
            int maxConnectedNodes = discoveryResponseMessages.Count == 0
                ? 0
                : discoveryResponseMessages.Max(m => m.NodeConnectionNum);

            DiscoveryResponseMessage maven = discoveryResponseMessages?.FirstOrDefault(message =>
                message.NodeConnectionNum == maxConnectedNodes);

            //await Console.Out.WriteLineAsync("Before maven null").ConfigureAwait(false);

            if (maven == null)
            {
                //await Console.Out.WriteLineAsync("Maven is NULL").ConfigureAwait(false);
                await Console.Out.WriteLineAsync("No nodes were discovered! Exiting application...")
                    .ConfigureAwait(false);
                Environment.Exit(1);
            }

            IPAddress mavenIpAddress = IPAddress.Parse(maven.IPAddress);
            IPEndPoint mavenEndPoint = new IPEndPoint(mavenIpAddress, maven.ListeningPort);

            return mavenEndPoint;
        }

        private async Task<LinkedList<DiscoveryResponseMessage>> ReceiveDiscoveryResponseMessagesAsync()
        {
            //var discoveryResponseMessages = await Task.Run(async () =>
            //{

            //}).ConfigureAwait(false);

            //return discoveryResponseMessages;

            var responseHandlers = new LinkedList<Task<DiscoveryResponseMessage>>();
            var discoveryResponseMessages = new LinkedList<DiscoveryResponseMessage>();

            Thread thread = new Thread(() =>
            {
                var tcpListener = new TcpListenerEx(IPAddress.Any, _discoveryResponsePort);

                try
                {
                    tcpListener.Start();

                    Console.WriteLine(" [TCP] The local End point is  :" + tcpListener.LocalEndpoint);
                    Console.WriteLine(" [TCP] Waiting for a connection.....\n");

                    int timeoutSec = 600;
                    TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(timeoutSec);
                    DateTime listeningStartTime = DateTime.Now;

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

                    TcpClient tcpClient = default;

                    while (DateTime.Now.Subtract(listeningStartTime) < timeoutTimeSpan
                    ) // is serving continuously while timeout isn't reached
                    {
                        Console.Out.WriteLine("Before accepting....");


                        try
                        {
                            tcpClient = tcpListener.AcceptTcpClient();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

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

                        Console.WriteLine($" [TCP] Connection accepted from: {{ {tcpClient.Client.RemoteEndPoint} }}");
                        Console.WriteLine($" [TCP] SoketWorker is bound to: {{ {tcpClient.Client.LocalEndPoint} }}");

                        #region Trash

                        //TcpServerWorker.Instance
                        //    .Init(workerTcpSocket, tcpListener)
                        //    .StartWorking();

                        //// TODO Unchecked modification
                        //if (tcpListener.Inactive)
                        //{
                        //    tcpListener.Stop();
                        //}

                        #endregion

                        Task<DiscoveryResponseMessage> responseHandlerTask1 = Task.Run(() =>
                        {
                            return HandleResponseAsync(tcpListener, tcpClient);
                        });

                        responseHandlers.AddLast(responseHandlerTask1);
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


            foreach (Task<DiscoveryResponseMessage> handlerTask in responseHandlers)
            {
                if (!handlerTask.IsCompleted)
                {
                    // We do not care about the tasks that didn't get the job done
                    continue;
                }

                DiscoveryResponseMessage discoveryResponseMessage = await handlerTask.ConfigureAwait(false);

                discoveryResponseMessages.AddLast(discoveryResponseMessage);
            }


            return discoveryResponseMessages;
        }

        private async Task<DiscoveryResponseMessage> HandleResponseAsync(
            TcpListenerEx tcpListener, TcpClient tcpWorker)
        {
            await Console.Out.WriteLineAsync(
                    $" [TCP]   >> SERVER WORKER IS TALKING TO {tcpWorker.Client.RemoteEndPoint}")
                .ConfigureAwait(false);

            LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

            while (true)
            {
                if (tcpListener.Inactive)
                {
                    break;
                }

                byte[] buffer = new byte[Common.BufferSize];

                //int receivedBytes = workerTcpSocket.Receive(buffer);

                NetworkStream networkStream = tcpWorker.GetStream();

                int receivedBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (receivedBytes == 0)
                {
                    tcpWorker.Close();

                    Console.Out.WriteLine(
                        $@" [TCP]   >> SERVER WORKER says: ""No bytes received. Connection closed.""");

                    break;
                }

                receivedBinaryData.AddLast(buffer.Take(receivedBytes));

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
            }

            tcpWorker.Close();

            #region Trash

            //if (serverMustStopServingRequests && _server.Active)
            //{
            //    _server.Stop();
            //    Console.Out.WriteLine(" [TCP] SERVER HALTED");
            //}

            #endregion

            byte[] data = receivedBinaryData.SelectMany(batch => batch).ToArray();

            string xmlData = data.ToUtf8String();

            var responseMessage = xmlData.DeserializeTo<DiscoveryResponseMessage>();

            // To be removed $C$
            await Console.Out.WriteLineAsync($" [TCP]   >> SERVER WORKER finished job").ConfigureAwait(false);

            return responseMessage;
        }


        private Task InitializeDiscoveryProcedureAsync(Socket mCastSocket)
        {
            var discoveryRequestMessage = new MulticastDiscoveryRequestMessage
            {
                IPAddress = ClientLocalIpAddress.ToString(),
                ListeningPort = _discoveryResponsePort
            };

            // Send multicast packets to the listener.
            IPEndPoint remoteEndPoint = new IPEndPoint(MulticastIPEndPoint.Address, MulticastIPEndPoint.Port);

            string xml = discoveryRequestMessage.SerializeToXml();
            byte[] dataToBeSent = xml.ToUtf8EncodedByteArray();

            mCastSocket.SendTo(dataToBeSent, remoteEndPoint);

            Console.WriteLine("Initializing discovery procedure...");

            mCastSocket.Close();

            return Task.CompletedTask;
        }

        private Task<Socket> CreateMulticastSocketAsync()
        {
            var mCastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var localEP = new IPEndPoint(ClientLocalIpAddress, 0);

            // Bind this endpoint to the multicast socket.
            mCastSocket.Bind(localEP); // bind socket to 127.0.0.1:*

            // Define a MulticastOption object specifying the multicast group address and the local IP address.
            // The multicast group address is the same as the address used by the listener.
            var mCastOption = new MulticastOption(MulticastIPEndPoint.Address, ClientLocalIpAddress);

            mCastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mCastOption);

            return Task.FromResult(mCastSocket);
        }

        #endregion

        #region Download Payload Data

        private async Task<string> RetrieveDataFromMavenAsync(
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
            string data = await dataAgent.MakeRequestAsync(requestMessage, mavenEndPoint)
                .ConfigureAwait(false);

            return data;
        }

        #endregion
    }
}