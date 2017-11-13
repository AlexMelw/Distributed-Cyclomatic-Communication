namespace DCCClientLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon.Messages;
    using Interfaces;

    public abstract class DCCClientWorker : IDCCClientWorker
    {
        protected string ReceivedData;
        public IPAddress LocalIpAddress { get; set; }
        public int ResponseTcpPort { get; set; }

        public IPEndPoint MulticastIPEndPoint { get; set; }

        public ICommunicationMediator CommunicationMediator { get; set; }

        public abstract Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath);

        public async Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            var requestMessage = new RequestDataMessage
            {
                Propagation = 1,
                DataType = dataType,
                FilterCondition = filterCondition,
                OrderingCondition = orderingCondition
            };

            ReceivedData = await CommunicationMediator.MakeRequestAsync(requestMessage).ConfigureAwait(false);
        }

        public Task<string> GetResponseAsync() => Task.FromResult(ReceivedData);

        public void Dispose() { }
    }
}