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
        public void SendDiscoveryResponse(
            DiscoveryResponseMessage responseMessage,
            IPAddress clientIpAddress, int clientListeningPort)
        {
            // $C$ Bug Fix - TO BE REVIEWED
            // Let the client initialize the response TcpListener
            //await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

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