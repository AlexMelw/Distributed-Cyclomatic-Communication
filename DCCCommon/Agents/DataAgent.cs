namespace DCCCommon.Agents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Conventions;
    using EasySharp.NHelpers.CustomExMethods;
    using Messages;

    public class DataAgent
    {
        public void SendResponse(Socket workerSocket, string serializedData)
        {
            // To be returned

            byte[] dataToBeSent = serializedData.ToUtf8EncodedByteArray();

            // Send header (meta-data) first

            string header = dataToBeSent.Length.ToString();
            Console.Out.WriteLine($"Payload to be sent: [ {header} ] bytes.");

            byte[] binaryHeader = header.ToUtf8EncodedByteArray();
            workerSocket.Send(binaryHeader);

            byte[] buffer = new byte[Common.UnicastBufferSize];
            int receivedBytes = workerSocket.Receive(buffer);
            byte[] binaryAck = buffer.Take(receivedBytes).ToArray();
            string ack = binaryAck.ToUtf8String();
            Console.Out.WriteLine($"Payload acknowledgment: [ {ack} ] bytes.");


            // Then send payload data

            Console.Out.WriteLine($"Sending data...");

            IEnumerable<IEnumerable<byte>> chunks = dataToBeSent.ChunkBy(Common.UnicastBufferSize);

            foreach (IEnumerable<byte> chunk in chunks)
            {
                byte[] chunkBuffer = chunk.ToArray();
                workerSocket.Send(chunkBuffer, SocketFlags.Partial);
            }
        }


        public string MakeRequest(RequestDataMessage requestMessage, IPEndPoint dataSourceEndPoint, string nodeId)
        {
            Console.Out.WriteLine($"[Node ID {nodeId}] Making data request to the maven node [ {dataSourceEndPoint} ]");

            // $c$ ADD THREAD OR TASK

            // Establish connection to the remote node
            var tcpClient = new TcpClient();
            tcpClient.Connect(dataSourceEndPoint.Address, dataSourceEndPoint.Port);
            Socket socket = tcpClient.Client;

            //NetworkStream networkStream = tcpClient.GetStream();

            Console.Out.WriteLine($"[Node ID {nodeId}] Successfully connected to [ {dataSourceEndPoint} ]");

            // Prepare request message to be sent
            string requestMessageXml = requestMessage.SerializeToXml();

            Console.Out.WriteLine($"[Node ID {nodeId}] Sending XML request");
            Console.Out.WriteLine(requestMessageXml);

            byte[] dataToBeSent = requestMessageXml.ToUtf8EncodedByteArray();

            #region Trash

            //var streamReader = new StreamReader(networkStream, Encoding.UTF8);
            //var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            //await streamWriter.WriteAsync(requestMessageXml).ConfigureAwait(false);

            #endregion

            // Send request message
            //networkStream.Write(dataToBeSent, 0, dataToBeSent.Length);
            socket.Send(dataToBeSent);

            Console.Out.WriteLine($"[Node ID {nodeId}] Message sent successfully");

            // Receive meta-data response
            Console.Out.WriteLine($"[Node ID {nodeId}] Receiving data");

            byte[] buffer = new byte[Common.UnicastBufferSize];
            //int bytesRead = networkStream.Read(buffer, 0, Common.UnicastBufferSize);
            int bytesRead = socket.Receive(buffer);

            //long payloadSize = BitConverter.ToInt64(buffer.Take(bytesRead).ToArray(), 0);
            byte[] binaryHeader = buffer.Take(bytesRead).ToArray();
            string header = binaryHeader.ToUtf8String();
            long payloadSize = Convert.ToInt64(header);
            Console.Out.WriteLine($"[Node ID {nodeId}] The payload size is [ {payloadSize} ] bytes.");

            byte[] ackBuffer = payloadSize.ToString().ToUtf8EncodedByteArray();
            socket.Send(ackBuffer);


            // Get Payload Data from maven
            //string data = RetrieveDataPayloadFromMaven(payloadSize, networkStream, buffer);
            string data = RetrieveDataPayloadFromMaven(socket, payloadSize);

            #region Trash

            //streamReader.Close();
            //streamWriter.Close();

            #endregion

            tcpClient.Close();

            return data;
        }

        private string RetrieveDataPayloadFromMaven(Socket socket, long payloadSize)
        {
            var receivedDataChunks = new LinkedList<IEnumerable<byte>>();

            while (payloadSize > 0)
            {
                //int bytesRead = networkStream.Read(buffer, 0, Common.UnicastBufferSize);

                byte[] buffer = new byte[Common.UnicastBufferSize];

                int bytesRead = socket.Receive(buffer, SocketFlags.Partial);

                receivedDataChunks.AddLast(buffer.Take(bytesRead));

                payloadSize -= bytesRead;
            }

            byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

            string data = receivedData.ToUtf8String();

            return data;
        }

        //private string RetrieveDataPayloadFromMaven(long payloadSize, NetworkStream networkStream,
        //    byte[] buffer)
        //{
        //    var receivedDataChunks = new LinkedList<IEnumerable<byte>>();

        //    while (payloadSize > 0)
        //    {
        //        int bytesRead = networkStream.Read(buffer, 0, Common.UnicastBufferSize);

        //        receivedDataChunks.AddLast(buffer.Take(bytesRead));

        //        payloadSize -= bytesRead;
        //    }

        //    byte[] receivedData = receivedDataChunks.SelectMany(chunk => chunk).ToArray();

        //    string data = receivedData.ToUtf8String();

        //    return data;
        //}
    }
}