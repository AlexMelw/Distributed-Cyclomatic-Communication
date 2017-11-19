namespace DCCClientLib.Workers
{
    using System;
    using System.IO;
    using Interfaces;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    public class DCCJsonClientWorker : DCCClientWorker, IDCCJsonClientWorker
    {
        public override bool ValidateResponseAgainstSchema(string schemaPath)
        {
            string jsonSchema = default;

            try
            {
                jsonSchema = File.ReadAllText(schemaPath);
            }
            catch (IOException)
            {
                Console.Out.WriteLine($"Error while reading file {schemaPath}.");
                Console.Out.WriteLine("Application terminated.");
                Environment.Exit(1);
            }

            JSchema schema = JSchema.Parse(jsonSchema);

            JObject json = JObject.Parse(ReceivedData);

            bool valid = json.IsValid(schema);

            return valid;
        }
    }
}