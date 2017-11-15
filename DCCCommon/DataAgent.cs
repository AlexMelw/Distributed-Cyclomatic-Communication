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
        public async Task<string> MakeRequestAsync(RequestDataMessage requestMessage, IPEndPoint SourceEndPoint)
        {
            // Establish connection to the remote node
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(SourceEndPoint.Address, SourceEndPoint.Port).ConfigureAwait(false);
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
            await networkStream.WriteAsync(dataToBeSent, 0, dataToBeSent.Length).ConfigureAwait(false);

            // Receive meta-data response
            byte[] buffer = new byte[Common.BufferSize];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);
            int payloadSize = BitConverter.ToInt32(buffer.Take(bytesRead).ToArray(), 0);

            // Get Payload Data from maven
            string data = await RetrieveDataPayloadFromMavenAsync(payloadSize, networkStream, buffer)
                .ConfigureAwait(false);

            #region Trash

            //streamReader.Close();
            //streamWriter.Close();

            #endregion

            tcpClient.Close();

            return data;
        }

        private async Task<string> RetrieveDataPayloadFromMavenAsync(int payloadSize, NetworkStream networkStream,
            byte[] buffer)
        {
            var receivedDataChunks = new LinkedList<IEnumerable<byte>>();

            while (payloadSize > 0)
            {
                int bytesRead = await networkStream.ReadAsync(buffer, 0, Common.BufferSize).ConfigureAwait(false);

                receivedDataChunks.AddLast(buffer.Take(bytesRead));

                payloadSize -= payloadSize;
            }

            byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

            string data = receivedData.ToUtf8String();

            return data;
        }
    }
}