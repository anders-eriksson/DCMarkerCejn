using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMarker.Flexible
{
    /// <summary>
    /// Class for each product item
    /// </summary>
    public class FlexibleItem
    {
        public int ItemId { get; set; }
        public int NumberOfEdges { get; set; }
        public int CurrentEdge { get; set; }
        public FlexibleItemStates ItemState { get; set; }

        public FlexibleItem()
        {
            CurrentEdge = 0;
            ItemState = FlexibleItemStates.None;
        }
    }
}