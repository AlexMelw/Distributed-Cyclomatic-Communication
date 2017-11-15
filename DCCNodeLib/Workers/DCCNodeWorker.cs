namespace DCCNodeLib.Workers
{
    using DSL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using DCCDiscoveryService.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using EasySharp.NHelpers.CustomWrappers.Networking;
    using Interfaces;
    using static DCCCommon.Conventions.Common;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        public int CurrentNodeId { get; set; }
        public string DataSourcePath { get; set; }
        public IPAddress LocalIpAddress { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int TcpServingPort { get; set; }
        public IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }

        public void Start()
        {
            Console.Out.WriteLine($"Node with id [ {CurrentNodeId} ] is activated.");

            Task multicastListenerTask = Task.Run(StartListeningToMulticastPortAsync);

            Task tcpListenerTask = Task.Run(StartListeningToTcpServingPortAsync);

            Task.WaitAll(multicastListenerTask, tcpListenerTask);
        }

        public async Task InitAsync(int nodeId)
        {
            CurrentNodeId = nodeId;

            string nodeDataSourcePath = StartupConfigManager.Default
                .GetNodeDataSourcePath(nodeId);

            if (string.IsNullOrWhiteSpace(nodeDataSourcePath))
            {
                await Console.Out
                    .WriteLineAsync("Data Source path hadn't been found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPAddress localIpAddress = StartupConfigManager.Default
                .GetNodeLocalIpAddress(nodeId);

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetNodeMulticastIPEndPoint(nodeId);

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            int tcpServingPort = StartupConfigManager.Default
                .GetNodeTcpServingPort(nodeId);

            if (tcpServingPort == -1)
            {
                await Console.Out
                    .WriteLineAsync("TCP serving port is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IEnumerable<IPEndPoint> adjacentNodesEndPoints = StartupConfigManager.Default
                .GetAdjacentNodesEndPoints(nodeId);

            DataSourcePath = nodeDataSourcePath;
            LocalIpAddress = localIpAddress;
            MulticastIPEndPoint = multicastIpEndPoint;
            TcpServingPort = tcpServingPort;
            AdjacentNodesEndPoints = adjacentNodesEndPoints;
        }

        private async Task StartListeningToMulticastPortAsync()
        {
            // Multicast Socket Initialization
            Socket mCastSocket = await MulticastSocketInitAsync().ConfigureAwait(false);

            // To be put below the while loop
            //mCastSocket.Close(300);

            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[MulticastBufferSize];

            while (true)
            {
                await Console.Out.WriteLineAsync("Waiting for multicast packets...").ConfigureAwait(false);
                await Console.Out.WriteLineAsync("Enter ^C to terminate").ConfigureAwait(false);

                int bytesRead = mCastSocket.ReceiveFrom(buffer, ref remoteEndPoint);

                await ProcessMulticastMessageAsync(buffer, bytesRead).ConfigureAwait(false);
            }
        }

        private Task<Socket> MulticastSocketInitAsync()
        {
            Socket mCastSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            var localEndPoint = new IPEndPoint(LocalIpAddress, MulticastIPEndPoint.Port);

            mCastSocket.Bind(localEndPoint);

            mCastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                new MulticastOption(MulticastIPEndPoint.Address, LocalIpAddress));

            return Task.FromResult(mCastSocket);
        }

        private async Task ProcessMulticastMessageAsync(byte[] messageBuffer, int bytesRead)
        {
            string xmlMessage = messageBuffer.Take(bytesRead).ToArray().ToUtf8String();

            var requestMessage = xmlMessage.DeserializeTo<MulticastDiscoveryRequestMessage>();

            var responseAgent = new ResponseAgent();

            var responseMessage = new DiscoveryResponseMessage
            {
                IPAddress = LocalIpAddress.ToString(),
                ListeningPort = TcpServingPort,
                NodeConnectionNum = AdjacentNodesEndPoints.Count()
            };

            var clientIpAddress = IPAddress.Parse(responseMessage.IPAddress);
            int clientListeningPort = requestMessage.ListeningPort;

            await responseAgent.SendDiscoveryResponseAsync(responseMessage, clientIpAddress, clientListeningPort)
                .ConfigureAwait(false);
        }

        private Task StartListeningToTcpServingPortAsync()
        {
            //var discoveryResponseMessages = new LinkedList<DiscoveryResponseMessage>();

            var tcpListener = new TcpListenerEx(IPAddress.Any, TcpServingPort);

            try
            {
                tcpListener.Start();

                Console.WriteLine(" [TCP] The local End point is  :" + tcpListener.LocalEndpoint);
                Console.WriteLine(" [TCP] Waiting for a connection.....\n");

                //var timeout = TimeSpan.FromSeconds(30);
                //DateTime listeningStartTime = DateTime.Now;

                //var responseHandlers = new LinkedList<Task<DiscoveryResponseMessage>>();

                while (true) // is serving continuously
                {
                    //if (DateTime.Now.Subtract(listeningStartTime) >= timeout)
                    //{
                    //    // No more accept responses from DIS nodes
                    //    break;
                    //}

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

                    //Task<DiscoveryResponseMessage> responseHandlerTask = Task.Run(() =>
                    //{
                    //    return HandleResponseAsync(tcpListener, tcpClient);
                    //});

                    //responseHandlers.AddLast(responseHandlerTask);

                    Task.Run(() => HandleRequestAsync(tcpListener, tcpClient));
                }


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

            return Task.CompletedTask;
        }

        private async Task HandleRequestAsync(TcpListenerEx tcpListener, TcpClient tcpWorker)
        {
            await Console.Out.WriteLineAsync(
                    $" [TCP]   >> SERVER WORKER IS TALKING TO {tcpWorker.Client.RemoteEndPoint}")
                .ConfigureAwait(false);

            LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

            NetworkStream networkStream = tcpWorker.GetStream();

            while (true)
            {
                if (tcpListener.Inactive)
                {
                    break;
                }

                byte[] buffer = new byte[BufferSize];

                int receivedBytes = await networkStream
                    .ReadAsync(buffer, 0, buffer.Length)
                    .ConfigureAwait(false);

                if (receivedBytes == 0)
                {
                    await Console.Out.WriteLineAsync(
                            $@" [TCP]   >> SERVER WORKER says: ""No bytes received. Connection closed.""")
                        .ConfigureAwait(false);

                    break;
                }

                receivedBinaryData.AddLast(buffer.Take(receivedBytes));
            }

            tcpWorker.Close();

            byte[] binaryMessage = receivedBinaryData.SelectMany(batch => batch).ToArray();

            string xmlMessage = binaryMessage.ToUtf8String();

            var requestDataMessage = xmlMessage.DeserializeTo<RequestDataMessage>();


            var dslInterpreter = new DSLInterpreter(requestDataMessage);

            IEnumerable<Employee> currentNodeEmployees = await dslInterpreter.GetDataAsync().ConfigureAwait(false);

            var employees = new List<Employee>(currentNodeEmployees);

            var dataAgentRequestTasks = new LinkedList<Task<string>>();

            if (requestDataMessage.Propagation > 0)
            {
                // Message Retransmission
                requestDataMessage.Propagation = 0;
                requestDataMessage.DataFormat = Xml;

                var dataAgent = new DataAgent();

                foreach (IPEndPoint nodeEndPoint in AdjacentNodesEndPoints)
                {
                    Task<string> dataRequestTask = dataAgent.MakeRequestAsync(requestDataMessage, nodeEndPoint);

                    dataAgentRequestTasks.AddLast(dataRequestTask);
                }
            }

            while (dataAgentRequestTasks.Count > 0)
            {
                foreach (Task<string> dataRequestTask in dataAgentRequestTasks)
                {
                    // Identify the first task that completes.
                    Task<string> firstCompletedTask = await Task.WhenAny(dataAgentRequestTasks).ConfigureAwait(false);

                    // Remove the selected task from the list so that you don't 
                    // process it more than once.
                    dataAgentRequestTasks.Remove(firstCompletedTask);

                    // Await the completed task.
                    string xmlData = await firstCompletedTask.ConfigureAwait(false);

                    var employeesContainer = xmlData.DeserializeTo<EmployeesRoot>();

                    employees.AddRange(employeesContainer.EmployeeArray);
                }
            }


            var dslConverter = new DSLConverter(requestDataMessage);

            string serializedData = await dslConverter
                .TransfromDataToRequiredFromatAsync(employees)
                .ConfigureAwait(false);

            // To be returned

            byte[] dataToBeSent = serializedData.ToUtf8EncodedByteArray();

            await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);

            tcpWorker.Close();
        }

        //public void Dispose() { }
    }
}