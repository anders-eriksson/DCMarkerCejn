using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleItems
{
    internal class Program
    {
        private static Item item1;
        private static Item item2;

        private static void Main(string[] args)
        {
            item1 = new Item(1);
            item1.Status = ItemStatus.None;
            item1.MaxEdge = 2;

            item2 = new Item(2);
            item2.Status = ItemStatus.None;
            item2.MaxEdge = 2;
        }
    }
}