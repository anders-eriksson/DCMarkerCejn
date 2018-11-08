using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class UpdateItemStatusArgs : EventArgs
    {
        public UpdateItemStatusArgs(FlexibleItem item)
        {
            Item = item;
        }

        public FlexibleItem Item { get; private set; }
    }
}