using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ItemDoneArgs : EventArgs
    {
        public ItemDoneArgs(int numberofItemsDone)
        {
            NumberofItemsDone = numberofItemsDone;
        }

        public int NumberofItemsDone { get; private set; }
    }
}