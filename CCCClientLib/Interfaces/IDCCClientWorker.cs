namespace DCCClientLib.Interfaces
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCClientWorker : IDisposable
    {
        ICommunicationMediator CommunicationMediator { get; set; }
        Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath);
        Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition);
        Task<string> GetResponseAsync();
        Task InitializeAsync();
    }
}