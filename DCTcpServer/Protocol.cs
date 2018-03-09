namespace DCTcpServer
{
    using Configuration;
    using System;
    using System.Linq;
    using System.Text;

    internal class Protocol
    {
        private byte[] _buffer;

        public Protocol(byte[] buffer)
        {
            _buffer = buffer;
            SplitIntoParam();
        }

        public string ArticleNumber { get; set; }
        public string TOnumber { get; set; }

        public void SplitIntoParam()
        {
            int nArticle = DCConfig.Instance.ArticleNumberLength;
            int nTOnumber = DCConfig.Instance.ToNumberLength;

            var articleNumber = string.Empty;
            var toNumber = string.Empty;

            if (_buffer.Length >= nArticle + nTOnumber)
            {
                byte[] articleNumberArray = new byte[nArticle];
                Array.Copy(_buffer, 0, articleNumberArray, 0, nArticle);
                int count = articleNumberArray.Count(bt => bt != 0); // find the first null
                articleNumber = System.Text.Encoding.ASCII.GetString(articleNumberArray, 0, count);
                ArticleNumber = RemoveNonPrintableChars(articleNumber);

                byte[] toNumberArray = new byte[nTOnumber];
                Array.Copy(_buffer, 0, toNumberArray, 0, nTOnumber);
                count = toNumberArray.Count(bt => bt != 0); // find the first null
                toNumber = System.Text.Encoding.ASCII.GetString(toNumberArray, 0, count);

                TOnumber = RemoveNonPrintableChars(toNumber);
            }
            else
            {
                int count = _buffer.Count(bt => bt != 0); // find the first null
                articleNumber = System.Text.Encoding.ASCII.GetString(_buffer, 0, count);

                ArticleNumber = RemoveNonPrintableChars(articleNumber);
                TOnumber = string.Empty;
            }
        }

        private string RemoveNonPrintableChars(string articleNumber)
        {
            StringBuilder sb = new StringBuilder();
            int len = articleNumber.Length;
            for (int i = 0; i < len; i++)
            {
                if (articleNumber[i] > 31)
                {
                    sb.Append(articleNumber[i]);
                }
            }

            return sb.ToString();
        }
    }
}