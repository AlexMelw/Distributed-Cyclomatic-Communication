namespace DCCClientLib.Interfaces
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCClientWorker
    {
        ICommunicationMediator CommunicationMediator { get; set; }
        Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath);
        Task MakeRequestAsync(string dataType, string dataFormat, string filterCondition, string orderingCondition);
        Task<string> GetResponseAsync();
        Task InitializeAsync();
    }
}