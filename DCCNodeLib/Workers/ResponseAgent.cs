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
        public async Task SendDiscoveryResponseAsync(
            DiscoveryResponseMessage responseMessage,
            IPAddress clientIpAddress, int clientListeningPort)
        {
            // $C$ Bug Fix - TO BE REVIEWED
            // Let the client initialize the response TcpListener
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            // Let's send the response
            var discoveryReceiverEP = new IPEndPoint(clientIpAddress, clientListeningPort);

            var tcpSender = new TcpClient(discoveryReceiverEP);

            NetworkStream networkStream = tcpSender.GetStream();

            string xmlMessage = responseMessage.SerializeToXml();

            byte[] dataToBeSent = xmlMessage.ToUtf8EncodedByteArray();

            await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);
        }
    }
}