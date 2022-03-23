namespace Contracts
{
    public class Article
    {
        public Article()
        {
            EnableTO = false;
            Careful = false;
            IsTestItemSelected = false;
        }

        public int Id { get; set; }
        public string F1 { get; set; }
        public bool IsNewArticleNumber { get; set; }
        public string Kant { get; set; }
        public string MaskinID { get; set; }
        public string FixtureId { get; set; }
        public bool? EnableTO { get; set; }
        public bool? Careful { get; set; }
        public string TOnumber { get; set; }
        public string Template { get; set; }
        public bool? IsTestItemSelected { get; set; }
        public bool? ExternTest { get; set; }
    }
}