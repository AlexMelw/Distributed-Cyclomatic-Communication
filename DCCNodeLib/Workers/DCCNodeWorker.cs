namespace DCCNodeLib.Workers
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
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using DCCDiscoveryService.Messages;
    using DSL;
    using EasySharp.NHelpers.CustomExMethods;
    using EasySharp.NHelpers.CustomWrappers.Networking;
    using Interfaces;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        private IPAddress _localIpAddress;
        public int CurrentNodeId { get; set; }

        public string DataSourcePath { get; set; }

        //public IPAddress LocalIpAddress { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }

        public int TcpServingPort { get; set; }
        //public IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }

        public void Start()
        {
            Console.Out.WriteLine($"Node with id [ {CurrentNodeId} ] is activated.");

            new Thread(StartListeningToMulticastPort).Start();
            new Thread(StartListeningToTcpServingPort).Start();


            //Task tcpListenerTask = Task.Run(StartListeningToTcpServingPortAsync);
            //Task.WaitAll(multicastListenerTask, tcpListenerTask);
        }

        public void Init(int nodeId)
        {
            CurrentNodeId = nodeId;

            string nodeDataSourcePath = StartupConfigManager.Default
                .GetNodeDataSourcePath(nodeId);

            if (string.IsNullOrWhiteSpace(nodeDataSourcePath))
            {
                Console.Out.WriteLine("Data Source path hadn't been found in the configuration file.");
                Environment.Exit(1);
            }

            //IPAddress localIpAddress = StartupConfigManager.Default
            //    .GetNodeLocalIpAddress(nodeId);

            //if (localIpAddress == null)
            //{
            //    Console.Out.WriteLine("Local IP Address is not found in the configuration file.");
            //    Environment.Exit(1);
            //}

            _localIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();


            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetNodeMulticastIPEndPoint(nodeId);

            if (multicastIpEndPoint == null)
            {
                Console.Out.WriteLine("Multicast IP Address and port are not found in the configuration file.");
                Environment.Exit(1);
            }

            int tcpServingPort = StartupConfigManager.Default
                .GetNodeTcpServingPort(nodeId);

            if (tcpServingPort == -1)
            {
                Console.Out.WriteLine("TCP serving port is not found in the configuration file.");
                Environment.Exit(1);
            }

            IEnumerable<(int, IPEndPoint)> adjacentNodesEndPointsWithIDs = StartupConfigManager.Default
                .GetAdjacentNodesEndPointsWithIDs(nodeId);

            DataSourcePath = nodeDataSourcePath;
            //LocalIpAddress = localIpAddress;
            MulticastIPEndPoint = multicastIpEndPoint;
            TcpServingPort = tcpServingPort;
            AdjacentNodesEndPointsWithIDs = adjacentNodesEndPointsWithIDs;
        }

        public IEnumerable<(int, IPEndPoint)> AdjacentNodesEndPointsWithIDs { get; set; }

        private void StartListeningToMulticastPort()
        {
            // Multicast Socket Initialization
            Socket mCastSocket = MulticastSocketInit();


            // To be put below the while loop
            //mCastSocket.Close(300);

            Console.Out.WriteLine($"Start listening to {MulticastIPEndPoint}");

            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[Common.MulticastBufferSize];

            while (true)
            {
                Console.Out.WriteLine("Waiting for multicast packets...");
                Console.Out.WriteLine("Enter ^C to terminate");

                int bytesRead = mCastSocket.ReceiveFrom(buffer, ref remoteEndPoint);

                ProcessMulticastMessage(buffer, bytesRead);
            }
        }

        private Socket MulticastSocketInit()
        {
            Socket mCastSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            var localEndPoint = new IPEndPoint(_localIpAddress, MulticastIPEndPoint.Port);

            mCastSocket.Bind(localEndPoint);

            MulticastOption multicastOption =
                new MulticastOption(MulticastIPEndPoint.Address, _localIpAddress);

            mCastSocket.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                multicastOption);

            Console.WriteLine("Current multicast group is: " + multicastOption.Group);
            Console.WriteLine("Current multicast local address is: " + multicastOption.LocalAddress);

            return mCastSocket;
        }

        private void ProcessMulticastMessage(byte[] messageBuffer, int bytesRead)
        {
            string xmlMessage = messageBuffer.Take(bytesRead).ToArray().ToUtf8String();

            var requestMessage = xmlMessage.DeserializeTo<MulticastDiscoveryRequestMessage>();

            Console.Out.WriteLine("The captured Discovery Request Message is:");
            Console.Out.WriteLine(requestMessage);

            var clientIpAddress = IPAddress.Parse(requestMessage.IPAddress);
            int clientListeningPort = requestMessage.ListeningPort;

            // GENERATE RESPONSE

            var responseAgent = new DiscoveryResponseAgent();

            var responseMessage = new DiscoveryResponseMessage
            {
                //IPAddress = LocalIpAddress.ToString(), // $c$ to be changed
                IPAddress = _localIpAddress.ToString(),
                ListeningPort = TcpServingPort,
                NodeConnectionNum = AdjacentNodesEndPointsWithIDs.Count()
            };

            Console.Out.WriteLine(
                $"Client said that he wants to get DISCOVERY Response at {clientIpAddress}:{clientListeningPort}");

            responseAgent.SendDiscoveryResponse(responseMessage, clientIpAddress, clientListeningPort);
        }

        private void StartListeningToTcpServingPort()
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

                    Socket workerSoket = tcpListener.AcceptSocket();

                    Console.WriteLine($" [TCP] Connection accepted from: {{ {workerSoket.RemoteEndPoint} }}");
                    Console.WriteLine($" [TCP] SoketWorker is bound to: {{ {workerSoket.LocalEndPoint} }}");

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
                    //    return HandleRequestAsync(tcpListener, workerSoket);
                    //});

                    //responseHandlers.AddLast(responseHandlerTask);

                    new Thread(() => HandleRequest(tcpListener, workerSoket)).Start();
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
        }

        private void HandleRequest(TcpListenerEx tcpListener, Socket workerSocket)
        {
            Console.Out.WriteLine($" [TCP]   >> SERVER WORKER IS TALKING TO {workerSocket.RemoteEndPoint}");

            LinkedList<IEnumerable<byte>> receivedBinaryData = new LinkedList<IEnumerable<byte>>();

            //NetworkStream networkStream = workerSoket.GetStream();

            byte[] buffer = new byte[Common.UnicastBufferSize];

            int receivedBytes = workerSocket.Receive(buffer);

            byte[] binaryMessage = buffer.Take(receivedBytes).ToArray();

            string xmlMessage = binaryMessage.ToUtf8String();

            var requestDataMessage = xmlMessage.DeserializeTo<RequestDataMessage>();

            #region Trash

            //while (true)
            //{
            //    if (tcpListener.Inactive)
            //    {
            //        break;
            //    }
            //    byte[] buffer = new byte[BufferSize];
            //    int receivedBytes = networkStream.Read(buffer, 0, buffer.Length);
            //    if (receivedBytes == 0)
            //    {
            //        Console.Out.WriteLine($@" [TCP]   >> SERVER WORKER says: ""No bytes received. Connection closed.""");
            //        break;
            //    }
            //    receivedBinaryData.AddLast(buffer.Take(receivedBytes));
            //}

            #endregion

            var dslInterpreter = new DSLInterpreter(requestDataMessage);

            IEnumerable<Employee> currentNodeEmployees = dslInterpreter.GetData();

            var employees = new List<Employee>(currentNodeEmployees);

            var dataAgentRequestTasks = new LinkedList<Task<string>>();

            if (requestDataMessage.Propagation > 0)
            {
                // Message Retransmission
                requestDataMessage.Propagation = 0;
                requestDataMessage.DataFormat = Common.Xml;

                var dataAgent = new DataAgent();

                foreach (var idEpPair in AdjacentNodesEndPointsWithIDs)
                {
                    Task<string> requestDataTask = Task.Run(() =>
                    {
                        string xmlData = dataAgent.MakeRequest(
                            requestDataMessage,
                            idEpPair.Item2, 
                            idEpPair.Item1.ToString());

                        return xmlData;
                    });

                    dataAgentRequestTasks.AddLast(requestDataTask);
                }

            }

            while (dataAgentRequestTasks.Count > 0)
            {
                // Identify the first task that completes.
                Task<string> firstCompletedTask = Task.WhenAny(dataAgentRequestTasks).Result;

                // Remove the selected task from the list so that you don't 
                // process it more than once.
                dataAgentRequestTasks.Remove(firstCompletedTask);

                // Await the completed task.
                string xmlData = firstCompletedTask.Result;

                var employeesContainer = xmlData.DeserializeTo<EmployeesRoot>();

                employees.AddRange(employeesContainer.EmployeeArray);
            }


            var dslConverter = new DSLConverter(requestDataMessage);

            string serializedData = dslConverter.TransformDataToRequiredFormat(employees);

            // To be returned

            byte[] dataToBeSent = serializedData.ToUtf8EncodedByteArray();

            IEnumerable<IEnumerable<byte>> chunks = dataToBeSent.ChunkBy(Common.UnicastBufferSize);

            foreach (IEnumerable<byte> chunk in chunks)
            {
                byte[] chunkBuffer = chunk.ToArray();
                workerSocket.Send(chunkBuffer, chunkBuffer.Length, SocketFlags.Partial);
            }

            workerSocket.Close();
        }
        //public void Dispose() { }
    }
}