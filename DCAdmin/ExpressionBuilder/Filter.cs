namespace DCAdmin.ExpressionBuilder
{
    public class Filter
    {
        public string PropertyName { get; set; }
        public Op Operation { get; set; }
        public object Value { get; set; }
    }
}