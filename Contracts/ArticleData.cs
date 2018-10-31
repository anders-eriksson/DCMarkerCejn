namespace Contracts
{
    public class ArticleData
    {
        public string ArticleNumber { get; set; }
        public bool IsNewArticleNumber { get; set; }
        public string TOnr { get; set; }
        public int BatchSize { get; set; }
        public bool TestItem { get; set; }
        public int CurrentItem { get; set; }
    }
}