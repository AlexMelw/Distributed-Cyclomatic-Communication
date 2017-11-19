namespace DCCClientLib.Interfaces
{
    public interface IDCCClientWorker
    {
        ICommunicationMediator CommunicationMediator { get; set; }
        bool ValidateResponseAgainstSchema(string schemaPath);

        void MakeRequest(string dataType, string dataFormat, string filterCondition, string orderingCondition,
            int discoveryTimeout);

        string GetResponse();
        void Initialize();
    }
}