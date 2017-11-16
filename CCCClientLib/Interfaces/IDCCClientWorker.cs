namespace DCCClientLib.Interfaces
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IDCCClientWorker
    {
        ICommunicationMediator CommunicationMediator { get; set; }
        bool ValidateResponseAgainstSchema(string schemaPath);
        void MakeRequest(string dataType, string dataFormat, string filterCondition, string orderingCondition);
        string GetResponse();
        void Initialize();
    }
}