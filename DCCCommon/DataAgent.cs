namespace DCCCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Conventions;
    using EasySharp.NHelpers.CustomExMethods;
    using Messages;

    public class DataAgent
    {
        public string MakeRequest(RequestDataMessage requestMessage, IPEndPoint sourceEndPoint)
        {
            // $c$ ADD THREAD OR TASK

            // Establish connection to the remote node
            var tcpClient = new TcpClient();
            tcpClient.Connect(sourceEndPoint.Address, sourceEndPoint.Port);

            NetworkStream networkStream = tcpClient.GetStream();

            // Prepare request message to be sent
            string requestMessageXml = requestMessage.SerializeToXml();
            byte[] dataToBeSent = requestMessageXml.ToUtf8EncodedByteArray();

            #region Trash

            //var streamReader = new StreamReader(networkStream, Encoding.UTF8);
            //var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            //await streamWriter.WriteAsync(requestMessageXml).ConfigureAwait(false);

            #endregion

            // Send request message
            networkStream.Write(dataToBeSent, 0, dataToBeSent.Length);

            // Receive meta-data response
            byte[] buffer = new byte[Common.BufferSize];
            int bytesRead = networkStream.Read(buffer, 0, Common.BufferSize);
            int payloadSize = BitConverter.ToInt32(buffer.Take(bytesRead).ToArray(), 0);

            // Get Payload Data from maven
            string data = RetrieveDataPayloadFromMaven(payloadSize, networkStream, buffer);

            #region Trash

            //streamReader.Close();
            //streamWriter.Close();

            #endregion

            tcpClient.Close();

            return data;
        }

        private string RetrieveDataPayloadFromMaven(int payloadSize, NetworkStream networkStream,
            byte[] buffer)
        {
            var receivedDataChunks = new LinkedList<IEnumerable<byte>>();

            while (payloadSize > 0)
            {
                int bytesRead = networkStream.Read(buffer, 0, Common.BufferSize);

                receivedDataChunks.AddLast(buffer.Take(bytesRead));

                payloadSize -= payloadSize;
            }

            byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

            string data = receivedData.ToUtf8String();

            return data;
        }
    }
}