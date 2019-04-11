using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class UpdateItemStatusArgs : EventArgs
    {
        public UpdateItemStatusArgs(FlexibleItem[] items, int currentItem)
        {
            Items = items;
            CurrentItem = currentItem;
        }

        public FlexibleItem[] Items { get; private set; }
        public int CurrentItem { get; set; }
    }
}