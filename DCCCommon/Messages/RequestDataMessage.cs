namespace DCCCommon.Messages
{
    public class RequestDataMessage
    {
        public int Propagation { get; set; }
        public string DataType { get; set; }
        public string FilterCondition { get; set; }
        public string OrderingCondition { get; set; }
    }
}