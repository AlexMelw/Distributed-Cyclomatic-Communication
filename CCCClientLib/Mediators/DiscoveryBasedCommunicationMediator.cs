namespace DCCClientLib.Mediators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
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
            _discoveryResponsePort = RNGUtil.Next(30_000, 60_000);
        }

        #endregion

        public async Task<string> MakeRequestAsync(RequestDataMessage requestMessage)
        {
            IPEndPoint mavenEndPoint = await GetMavenEndPointAsync().ConfigureAwait(false);

            string data = await RetrieveDataFromMavenAsync(mavenEndPoint, requestMessage).ConfigureAwait(false);

            return data;
        }

        #region Maven Node Related

        private async Task<IPEndPoint> GetMavenEndPointAsync()
        {
            // JoinMulticastGroup
            var mCastSocket = await CreateMulticastSocketAsync().ConfigureAwait(false);

            // Discovery Init
            await InitializeDiscoveryProcedureAsync(mCastSocket).ConfigureAwait(false);

            // Discovery Receive Response
            var discoveryResponseMessages = await ReceiveDiscoveryResponseMessagesAsync().ConfigureAwait(false);

            // Discovery Proccess Response Results
            var mavenEndPoint = await IdentifyMavenNodeAsync(discoveryResponseMessages).ConfigureAwait(false);

            return mavenEndPoint;
        }

        private Task<IPEndPoint> IdentifyMavenNodeAsync(LinkedList<DiscoveryResponseMessage> discoveryResponseMessages)
        {
            int maxConnectedNodes = discoveryResponseMessages.Max(m => m.NodeConnectionNum);

            DiscoveryResponseMessage maven = discoveryResponseMessages?.FirstOrDefault(message =>
                message.NodeConnectionNum == maxConnectedNodes);

            IPAddress mavenIpAddress = IPAddress.Parse(maven.IPAddress);

            IPEndPoint mavenEndPoint = new IPEndPoint(mavenIpAddress, maven.ListeningPort);

            return Task.FromResult(mavenEndPoint);
        }

        private async Task<LinkedList<DiscoveryResponseMessage>> ReceiveDiscoveryResponseMessagesAsync()
        {
            var discoveryResponseMessages = new LinkedList<DiscoveryResponseMessage>();

            var tcpListener = new TcpListenerEx(IPAddress.Any, ClientReceiveResponseTcpPort);

            try
            {
                tcpListener.Start();

                Console.WriteLine(" [TCP] The local End point is  :" + tcpListener.LocalEndpoint);
                Console.WriteLine(" [TCP] Waiting for a connection.....\n");

                var timeout = TimeSpan.FromSeconds(30);
                DateTime listeningStartTime = DateTime.Now;

                var responseHandlers = new LinkedList<Task<DiscoveryResponseMessage>>();

                while (true) // is serving continuously
                {
                    if (DateTime.Now.Subtract(listeningStartTime) >= timeout)
                    {
                        // No more accept responses from DIS nodes
                        break;
                    }

                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

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

                    Task<DiscoveryResponseMessage> responseHandlerTask = Task.Run(() =>
                    {
                        return HandleResponseAsync(tcpListener, tcpClient);
                    });

                    responseHandlers.AddLast(responseHandlerTask);
                }


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
            }
            finally
            {
                if (tcpListener.Active)
                {
                    tcpListener.Stop();
                }
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
            IPEndPoint mavenEndPoint, RequestDataMessage requestMessage)
        {
            // Establish connection to the maven node
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(mavenEndPoint.Address, mavenEndPoint.Port).ConfigureAwait(false);
            NetworkStream networkStream = tcpClient.GetStream();

            // Prepare request message to be sent
            string requestMessageXml = requestMessage.SerializeToXml();
            byte[] dataToBeSent = requestMessageXml.ToUtf8EncodedByteArray();

            #region Trash

            //var streamReader = new StreamReader(networkStream, Encoding.UTF8);
            //var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            //await streamWriter.WriteAsync(requestMessageXml).ConfigureAwait(false);

            #endregion

            // Send request message
            await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);

            // Receive meta-data response
            byte[] buffer = new byte[Common.BufferSize];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);
            int payloadSize = BitConverter.ToInt32(buffer.Take(bytesRead).ToArray(), 0);

            // Get Payload Data from maven
            string data = await RetrieveDataPayloadFromMavenAsync(payloadSize, networkStream, buffer).ConfigureAwait(false);

            #region Trash

            //streamReader.Close();
            //streamWriter.Close();

            #endregion

            tcpClient.Close();

            return data;
        }

        private async Task<string> RetrieveDataPayloadFromMavenAsync(int payloadSize, NetworkStream networkStream, byte[] buffer)
        {
            var receivedDataChunks = new LinkedList<IEnumerable<byte>>();

            while (payloadSize > 0)
            {
                int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);

                receivedDataChunks.AddLast(buffer.Take(bytesRead));

                payloadSize -= payloadSize;
            }

            byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

            string data = receivedData.ToUtf8String();

            return data;
        }

        #endregion
    }
}