namespace DCCClientLib.Workers
{
    using System.Threading.Tasks;
    using Interfaces;

    public class DCCXmlClientWorker : DCCClientWorker, IDCCXmlClientWorker
    {
        public override Task<bool> ValidateResponseAgainstSchemaAsync(string xmlSchemaPath)
        {
            return base.ValidateResponseAgainstSchemaAsync(xmlSchemaPath);
        }
    }
}