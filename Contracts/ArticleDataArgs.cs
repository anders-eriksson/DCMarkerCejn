using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ArticleDataArgs : EventArgs
    {
        public ArticleDataArgs(ArticleData data)
        {
            Data = data;
        }

        public ArticleData Data { get; private set; }
    }
}