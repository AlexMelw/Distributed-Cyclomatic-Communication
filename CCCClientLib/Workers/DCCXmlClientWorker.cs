namespace DCCClientLib.Workers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Interfaces;

    public class DCCXmlClientWorker : DCCClientWorker, IDCCXmlClientWorker
    {
        public override Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath)
        {
            XDocument xDoc = XDocument.Parse(ReceivedData);


            XmlTextReader reader = new XmlTextReader(schemaPath);
            XmlSchema schema = XmlSchema.Read(reader, (sender, args) => { });

            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(schema);

            bool result = true;
            xDoc.Validate(schemaSet, (sender, args) => result = false);

            return Task.FromResult(result);
        }
    }
}