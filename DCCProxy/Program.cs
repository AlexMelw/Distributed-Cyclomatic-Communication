namespace DCCProxy
{
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new Proxy();

            proxy.Init();
            proxy.StartServingTcpPort();
        }
    }
}