using Contracts;
using System;

namespace DCMarker.Model
{
    internal class Co208ArticleInput : IArticleInput
    {
        public void CreateCo208ArticleInput(string article, int batchSize, bool testItem)
        {
            ArticleData data = new ArticleData
            {
                ArticleNumber = article,
                BatchSize = batchSize,
                TestItem = testItem
            };

            RaiseArticleEvent(null, data);
        }

        #region Article Event

        public event EventHandler<ArticleArgs> ArticleEvent;

        internal void RaiseArticleEvent(object sender, ArticleData data)
        {
            EventHandler<ArticleArgs> handler = ArticleEvent;
            if (handler != null)
            {
                ArticleArgs args = new ArticleArgs(data);
                handler(sender, args);
            }
        }

        public void Close()
        {
        }

        #endregion Article Event
    }
}