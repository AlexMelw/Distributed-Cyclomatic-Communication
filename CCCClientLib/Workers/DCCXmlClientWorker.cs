namespace DCCClientLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCDiscoveryService.Interfaces;
    using Interfaces;
    using Ninject;

    public class DCCXmlClientWorker : IDCCXmlClientWorker
    {
        public IPAddress LocalIpAddress { get; set; }

        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int ResponseTcpPort { get; set; }

        [Inject]
        public IDiscoveryService DiscoveryService { get; set; }

        //public DCCXmlClientWorker(IDiscoveryService discoveryService)
        //{
        //    DiscoveryService = discoveryService;
        //}

        public void Dispose() { }

        public Task<bool> ValidateResponseAgainstSchemaAsync(string xmlSchemaPath)
        {
            throw new NotImplementedException();
        }

        public async Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            try
            {
                IPEndPoint mavIpEndPoint = await DiscoveryService.GetMavenEndPointAsync().ConfigureAwait(false);
            }
            finally
            {
                DiscoveryService.Dispose();
            }
        }

        public Task<string> GetResponseAsync()
        {
            throw new NotImplementedException();
        }
    }
}