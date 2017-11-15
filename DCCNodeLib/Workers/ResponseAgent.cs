namespace DCCNodeLib.Workers
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using DCCDiscoveryService.Messages;
    using EasySharp.NHelpers.CustomExMethods;

    public class ResponseAgent
    {
        public Task SendDiscoveryResponseAsync(
            DiscoveryResponseMessage responseMessage,
            IPAddress clientIpAddress, int clientListeningPort)
        {
            var discoveryReceiverEP = new IPEndPoint(clientIpAddress, clientListeningPort);

            var tcpSender = new TcpClient(discoveryReceiverEP);

            NetworkStream networkStream = tcpSender.GetStream();

            string xmlMessage = responseMessage.SerializeToXml();

            byte[] dataToBeSent = xmlMessage.ToUtf8EncodedByteArray();

            networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length);

            return Task.CompletedTask;
        }
    }
}