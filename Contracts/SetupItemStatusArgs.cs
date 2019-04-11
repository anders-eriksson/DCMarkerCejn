using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class SetupItemStatusArgs : EventArgs
    {
        public SetupItemStatusArgs(FlexibleItem[] items)
        {
            Items = items;
        }

        public FlexibleItem[] Items { get; private set; }
    }
}