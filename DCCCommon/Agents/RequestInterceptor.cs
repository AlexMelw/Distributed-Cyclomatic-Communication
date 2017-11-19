namespace DCCCommon.Agents
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using Conventions;
    using EasySharp.NHelpers.CustomExMethods;
    using Messages;

    public class RequestInterceptor
    {
        public RequestDataMessage GetRequest(Socket workerSocket)
        {
            Console.Out.WriteLine($" [TCP]   >> SERVER WORKER IS TALKING TO {workerSocket.RemoteEndPoint}");

            byte[] buffer = new byte[Common.UnicastBufferSize];

            int receivedBytes = workerSocket.Receive(buffer);

            byte[] binaryMessage = buffer.Take(receivedBytes).ToArray();

            string xmlMessage = binaryMessage.ToUtf8String();

            Console.Out.WriteLine("Received Data Request Message");
            Console.Out.WriteLine(xmlMessage);

            var requestDataMessage = xmlMessage.DeserializeTo<RequestDataMessage>();
            return requestDataMessage;
        }
    }
}