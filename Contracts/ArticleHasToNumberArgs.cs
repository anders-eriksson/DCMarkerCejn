using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ArticleHasToNumberArgs : EventArgs
    {
        public ArticleHasToNumberArgs(bool state)
        {
            State = state;
        }

        public bool State { get; private set; }
    }
}