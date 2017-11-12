namespace DCCClientLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Interfaces;

    public class DCCJsonClientWorker : IDCCJsonClientWorker
    {
        public IPAddress LocalIpAddress { get; set; }

        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int ResponseTcpPort { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateResponseAgainstSchemaAsync(string xmlSchemaPath)
        {
            throw new NotImplementedException();
        }

        public Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResponseAsync()
        {
            throw new NotImplementedException();
        }
    }
}