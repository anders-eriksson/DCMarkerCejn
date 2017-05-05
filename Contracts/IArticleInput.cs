using System;

namespace Contracts
{
    public interface IArticleInput
    {
        event EventHandler<ArticleArgs> ArticleEvent;

        void Close();
    }
}