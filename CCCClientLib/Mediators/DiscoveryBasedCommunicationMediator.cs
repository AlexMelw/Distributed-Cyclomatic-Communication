namespace DCCClientLib.Mediators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
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

        public void Dispose() { }

        private async Task<string> RetrieveDataFromMavenAsync(IPEndPoint mavenEndPoint,
            RequestDataMessage requestMessage)
        {
            var transport = new TcpClient();
            await transport.ConnectAsync(mavenEndPoint.Address, mavenEndPoint.Port).ConfigureAwait(false);

            NetworkStream networkStream = transport.GetStream();

            string requestMessageXml = requestMessage.SerializeToXml();
            byte[] dataToBeSent = requestMessageXml.ToUtf8EncodedByteArray();

            //var streamReader = new StreamReader(networkStream, Encoding.UTF8);
            //var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            //await streamWriter.WriteAsync(requestMessageXml).ConfigureAwait(false);

            await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);

            byte[] buffer = new byte[Common.BufferSize];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);

            int payloadSize = BitConverter.ToInt32(buffer.Take(bytesRead).ToArray(), 0);

            LinkedList<IEnumerable<byte>> receivedDataChunks = new LinkedList<IEnumerable<byte>>();

            while (payloadSize > 0)
            {
                bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);

                receivedDataChunks.AddLast(buffer.Take(bytesRead));

                payloadSize -= payloadSize;
            }

            byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

            string data = receivedData.ToUtf8String();

            //streamReader.Close();
            //streamWriter.Close();

            return data;
        }

        private async Task<DiscoveryResponseMessage> HandleResponseAsync(
            TcpListenerEx tcpListener, TcpClient worker)
        {
            Console.Out.WriteLine($" [TCP]   >> SERVER WORKER IS TALKING TO {worker.Client.RemoteEndPoint}");

            LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

            while (true)
            {
                if (tcpListener.Inactive)
                {
                    break;
                }

                byte[] buffer = new byte[Common.BufferSize];

                //int receivedBytes = workerTcpSocket.Receive(buffer);

                NetworkStream networkStream = worker.GetStream();

                int receivedBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (receivedBytes == 0)
                {
                    worker.Close();

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

            worker.Close();

            #region Trash

            //if (serverMustStopServingRequests && _server.Active)
            //{
            //    _server.Stop();
            //    Console.Out.WriteLine(" [TCP] SERVER HALTED");
            //}

            #endregion

            byte[] data = receivedBinaryData.SelectMany(buffPart => buffPart).ToArray();

            string xmlData = data.ToUtf8String();

            var responseMessage = xmlData.DeserializeTo<DiscoveryResponseMessage>();

            // To be removed $C$
            await Console.Out.WriteLineAsync($" [TCP]   >> SERVER WORKER finished job").ConfigureAwait(false);

            return responseMessage;
        }

        private async Task<IPEndPoint> GetMavenEndPointAsync()
        {
            var discoveryRequestMessage = new MulticastDiscoveryRequestMessage
            {
                IPAddress = ClientLocalIpAddress.ToString(),
                ListeningPort = _discoveryResponsePort
            };

            #region JoinMulticastGroup

            var mCastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var localEP = new IPEndPoint(ClientLocalIpAddress, 0);

            // Bind this endpoint to the multicast socket.
            mCastSocket.Bind(localEP); // bind socket to 127.0.0.1:*

            // Define a MulticastOption object specifying the multicast group address and the local IP address.
            // The multicast group address is the same as the address used by the listener.
            var mCastOption = new MulticastOption(MulticastIPEndPoint.Address, ClientLocalIpAddress);

            mCastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mCastOption);

            #endregion

            #region Discovery Init

            // Send multicast packets to the listener.
            IPEndPoint remoteEndPoint = new IPEndPoint(MulticastIPEndPoint.Address, MulticastIPEndPoint.Port);

            string xml = discoveryRequestMessage.SerializeToXml();
            byte[] dataToBeSent = xml.ToUtf8EncodedByteArray();

            mCastSocket.SendTo(dataToBeSent, remoteEndPoint);

            Console.WriteLine("Initializing discovery procedure...");

            mCastSocket.Close();

            #endregion

            #region Discovery Receive Response

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

                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);

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

                    var responseHandlerTask = Task.Run(() => HandleResponseAsync(tcpListener, tcpClient));
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

            #endregion

            #region Discovery Processs Reqponse

            int maxConnectedNodes = discoveryResponseMessages.Max(m => m.NodeConnectionNum);

            DiscoveryResponseMessage maven = discoveryResponseMessages?.FirstOrDefault(message =>
                message.NodeConnectionNum == maxConnectedNodes);

            IPAddress mavenIpAddress = IPAddress.Parse(maven.IPAddress);

            IPEndPoint mavenEndPoint = new IPEndPoint(mavenIpAddress, maven.ListeningPort);

            return mavenEndPoint;

            #endregion
        }
    }
}