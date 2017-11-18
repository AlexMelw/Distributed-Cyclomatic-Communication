namespace DCCCommon.Agents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using Conventions;
    using EasySharp.NHelpers.CustomExMethods;

    public class DataRespondent
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
    }
}