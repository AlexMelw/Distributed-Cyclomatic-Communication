namespace DCCClientLib.Workers
{
    using System.Threading.Tasks;
    using Interfaces;

    public class DCCJsonClientWorker : DCCClientWorker, IDCCJsonClientWorker
    {
        public override Task<bool> ValidateResponseAgainstSchemaAsync(string xmlSchemaPath)
        {
            return base.ValidateResponseAgainstSchemaAsync(xmlSchemaPath);
        }
    }
}