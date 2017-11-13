namespace DCCClientLib.Interfaces
{
    using System.Threading.Tasks;
    using DCCCommon.Messages;

    public interface ICommunicationMediator
    {
        Task<string> MakeRequestAsync(RequestDataMessage requestMessage);
    }
}