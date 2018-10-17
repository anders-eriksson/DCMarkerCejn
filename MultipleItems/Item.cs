using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleItems
{
    public enum ItemStatus
    {
        None,
        ItemInPlace,
        Marking,
    }

    public class Item
    {
        public Item(int number)
        {
            ItemNumber = number;
        }

        public int ItemNumber { get; set; }
        public ItemStatus Status { get; set; }
        public int CurrentEdge { get; set; }
        public int MaxEdge { get; set; }
    }
}