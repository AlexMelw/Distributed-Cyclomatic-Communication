namespace DCCClientLib.Workers
{
    using System.Threading.Tasks;
    using Interfaces;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    public class DCCJsonClientWorker : DCCClientWorker, IDCCJsonClientWorker
    {
        public override Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath)
        {
            JSchema schema = JSchema.Parse(schemaPath);

            JObject json = JObject.Parse(ReceivedData);

            bool valid = json.IsValid(schema);

            return Task.FromResult(valid);
        }
    }
}