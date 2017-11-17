namespace DCCCommon.Messages
{
    using NClone;

    public class RequestDataMessage
    {
        public int Propagation { get; set; }
        public string DataFormat { get; set; }
        public string DataType { get; set; }
        public string FilterCondition { get; set; }
        public string OrderingCondition { get; set; }

        public override string ToString()
        {
            return $"{nameof(Propagation)}: {Propagation}, " +
                   $"{nameof(DataType)}: {DataType}, " +
                   $"{nameof(FilterCondition)}: {FilterCondition}, " +
                   $"{nameof(OrderingCondition)}: {OrderingCondition}";
        }

        public RequestDataMessage Replicate()
        {
            return Clone.ObjectGraph(this);
        }
    }
}