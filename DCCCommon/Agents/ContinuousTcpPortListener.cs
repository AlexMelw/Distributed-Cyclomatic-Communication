namespace DCCCommon.Agents
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using EasySharp.NHelpers.CustomWrappers.Networking;

    public class ContinuousTcpPortListener
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

                while (true) // is serving continuously
                {
                    Socket workerSoket = tcpListener.AcceptSocket();

                    Console.WriteLine($" [TCP] Connection accepted from: {{ {workerSoket.RemoteEndPoint} }}");
                    Console.WriteLine($" [TCP] SoketWorker is bound to: {{ {workerSoket.LocalEndPoint} }}");

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