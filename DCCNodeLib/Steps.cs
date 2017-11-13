namespace DCCNodeLib
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    //[XmlRoot("StepList")]
    //public class StepList
    //{
    //    [XmlElement("Step")]
    //    public List<Step> Steps { get; set; }
    //}

    public class Step
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Desc")]
        public string Desc { get; set; }
    }
}