using System;

namespace Contracts
{
    public class ArticleArgs : EventArgs
    {
        public ArticleArgs(ArticleData data)
        {
            Data = data;
        }

        public ArticleData Data { get; private set; } // readonly
    }
}
