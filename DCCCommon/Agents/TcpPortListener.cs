namespace DCCCommon.Agents
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using EasySharp.NHelpers.CustomWrappers.Networking;

    public class TcpPortListener
    {
        public void StartListening(int tcpServingPort, Action<Socket> handleRequestAction)
        {
            var tcpListener = new TcpListenerEx(IPAddress.Any, tcpServingPort);
            Console.Out.WriteLine($"TcpListener is active? [ {tcpListener.Active} ]");

            try
            {
                tcpListener.Start();
                Console.Out.WriteLine($"TcpListener is active? [ {tcpListener.Active} ]");
                Console.Out.WriteLine($"Listening at {IPAddress.Any}:{tcpServingPort}");

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

                    new Thread(() => handleRequestAction(workerSoket)).Start();
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("[TCP] Grave error occurred. Server is dead.");
                Console.Out.WriteLine($"e = {e.Message}");
                Debug.WriteLine("[TCP] Grave error occurred. Server is dead.");
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
    }
}