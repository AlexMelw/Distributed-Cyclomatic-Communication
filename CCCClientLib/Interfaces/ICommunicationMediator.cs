namespace DCCClientLib.Interfaces
{
    using System.Threading.Tasks;

    public interface ICommunicationMediator
    {
        Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition);
    }
}