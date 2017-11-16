namespace DCCClientLib.Interfaces
{
    using DCCCommon.Messages;

    public interface ICommunicationMediator
    {
        string MakeRequest(RequestDataMessage requestMessage);
    }
}