namespace Contracts
{
    public class LaserObjectData
    {
        public string ID { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("ID: {0}, Value: {1}", ID, Value);
        }
    }
}