namespace DCCClientLib.Interfaces
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCClientWorker : IDisposable
    {
        Task<bool> ValidateResponseAgainstSchemaAsync(string xmlSchemaPath);
        Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition);
        Task<string> GetResponseAsync();
        IPAddress LocalIpAddress { get; set; }
        IPEndPoint MulticastIPEndPoint { get; set; }
    }
}