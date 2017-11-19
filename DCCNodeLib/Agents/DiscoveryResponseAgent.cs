namespace DCCNodeLib.Agents
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using DCCCommon.Messages;
    using EasySharp.NHelpers.CustomExMethods;

    public class DiscoveryResponseAgent
    {
        public void SendDiscoveryResponse(
            DiscoveryResponseMessage responseMessage,
            IPAddress clientIpAddress, int clientListeningPort)
        {
            // Let's send the response
            var tcpSender = new TcpClient();
            tcpSender.Connect(clientIpAddress, clientListeningPort);

            NetworkStream networkStream = tcpSender.GetStream();

            string xmlMessage = responseMessage.SerializeToXml();

            Console.Out.WriteLine("I'm sending to client the following message:");
            Console.Out.WriteLine(xmlMessage);

            byte[] dataToBeSent = xmlMessage.ToUtf8EncodedByteArray();

            networkStream.Write(dataToBeSent, 0, dataToBeSent.Length);

            Console.Out.WriteLine("Message has been successfully sent to client");
        }
    }
}