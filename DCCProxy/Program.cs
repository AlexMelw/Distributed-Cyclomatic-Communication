namespace DCCProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using DCCCommon.Comparers;
    using DCCCommon.Entities;

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