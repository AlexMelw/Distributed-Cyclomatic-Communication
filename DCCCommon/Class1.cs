namespace DCCCommon
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class Class1
    {
        public static async Task Foo(string[] args)
        {
            //var tcpClient = new TcpClient();

            //NetworkStream networkStream = tcpClient.GetStream();

            //int size = tcpClient.ReceiveBufferSize;
            //byte[] buffer = new byte[size];
            //int readBytes = await networkStream.ReadAsync(buffer, 0, size).ConfigureAwait(false);

            //LinkedList<IEnumerable<byte>> lala = new LinkedList<IEnumerable<byte>>();
            //lala.AddLast(buffer.Take(readBytes));

            //byte[] totalBuff = lala.SelectMany(buffPart => buffPart).ToArray();
        }
    }
}