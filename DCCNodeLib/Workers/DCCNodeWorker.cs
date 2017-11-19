namespace DCCNodeLib.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;
    using DCCCommon;
    using DCCCommon.Agents;
    using DCCCommon.Comparers;
    using DCCCommon.Conventions;
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using DSL;
    using EasySharp.NHelpers.CustomExMethods;
    using Interfaces;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        private IPAddress _localIpAddress;
        public int CurrentNodeId { get; set; }
        public string DataSourcePath { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int TcpServingPort { get; set; }

        public void Start()
        {
            Console.Out.WriteLine($"Node with id [ {CurrentNodeId} ] is activated.");

            new Thread(StartListeningToMulticastPort).Start();
            new Thread(StartListeningToTcpServingPort).Start();
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

            mCastSocket.ExclusiveAddressUse = false;
            mCastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

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
            var portListener = new ContinuousTcpPortListener();
            portListener.StartListening(TcpServingPort, HandleRequest);
        }

        private void HandleRequest(Socket workerSocket)
        {
            #region Get Request Data Message

            RequestDataMessage requestDataMessage = new RequestInterceptor().GetRequest(workerSocket);

            #endregion

            #region Business Logic :: To be wrapped into DSLInterpreter

            var employees = Enumerable.Empty<Employee>().ToList();

            CollectDataFromAdjacentNodesIfRequired(requestDataMessage, employees);

            IEnumerable<Employee> dataFromCurrentNode = LocalStorageManager.Default
                                                            .GetEmployeesFrom(DataSourcePath)
                                                        ?? Enumerable.Empty<Employee>();


            employees.AddRange(dataFromCurrentNode);

            if (requestDataMessage.Propagation > 0)
            {
                employees = employees.Distinct(EmployeeIdComparer.Default).ToList();
            }


            employees = new DSLProcessor(requestDataMessage).ProcessData(employees).ToList();

            string serializedData = new DSLConverter(requestDataMessage)
                .TransformDataToRequiredFormat(employees);

            #endregion

            #region Send Back Response Data

            new DataAgent().SendResponse(workerSocket, serializedData);

            #endregion

            workerSocket.Close();
        }

        private void CollectDataFromAdjacentNodesIfRequired(RequestDataMessage requestDataMessage,
            List<Employee> employees)
        {
            var dataAgentRequestTasks = new LinkedList<Task<string>>();

            if (AdjacentNodesEndPointsWithIDs.Any() && requestDataMessage.Propagation > 0)
            {
                // Message Retransmission
                RequestDataMessage replicatedMessage = requestDataMessage.Replicate();

                replicatedMessage.Propagation = 0;
                replicatedMessage.DataFormat = Common.Xml;

                var dataAgent = new DataAgent();

                foreach (var idEpPair in AdjacentNodesEndPointsWithIDs)
                {
                    Task<string> requestDataTask = Task.Run<string>(() =>
                    {
                        string xmlData = dataAgent.MakeRequest(
                            replicatedMessage,
                            idEpPair.Item2,
                            idEpPair.Item1.ToString());

                        return xmlData;
                    });

                    dataAgentRequestTasks.AddLast(requestDataTask);
                }
            }

            Task.WaitAll(dataAgentRequestTasks.Cast<Task>().ToArray()); // WARNING: $C$ IMPORTANT CHANGES

            foreach (Task<string> task in dataAgentRequestTasks)
            {
                string xmlData = task.Result;

                EmployeesRoot root = xmlData.DeserializeTo<EmployeesRoot>();

                employees.AddRange(root.EmployeeArray);
            }
        }
    }
}