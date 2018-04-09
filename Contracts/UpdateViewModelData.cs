namespace Contracts
{
    public class UpdateViewModelData
    {
        public string Fixture { get; set; }
        public bool HasFixture { get; set; }
        public string ArticleNumber { get; set; }
        public bool Provbit { get; set; }
        public bool HasKant { get; set; }
        public string Kant { get; set; }
        public bool HasBatchSize { get; set; }
        public bool HasTOnr { get; set; }
        public bool NeedUserInput { get; set; }
        public string Status { get; set; }
        public string Template { get; set; }
    }
}