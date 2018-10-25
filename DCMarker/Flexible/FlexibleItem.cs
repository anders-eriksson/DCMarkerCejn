using Contracts;
using System.Collections.Generic;

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
        public List<Article> Articles { get; set; }
        public FlexibleItemStates ItemState { get; set; }

        public FlexibleItem()
        {
            ItemId = 0;
            CurrentEdge = 0;
            NumberOfEdges = 0;
            Articles = new List<Article>();
            ItemState = FlexibleItemStates.None;
        }

        /// <summary>
        /// Set values of Item
        /// </summary>
        /// <param name="itemid">Sequence number of current batch</param>
        /// <param name="numberofedges">Number of Edges that should be marked</param>
        /// <param name="currentedge">Current edge that will be marked</param>
        /// <param name="articles">Article data that will be used for marking</param>
        /// <param name="state">Item state</param>
        public void Set(int itemid, int numberofedges, int currentedge, List<Article> articles, FlexibleItemStates state)
        {
            ItemId = itemid;
            NumberOfEdges = numberofedges;
            CurrentEdge = currentedge;
            Articles = articles;
            ItemState = state;
        }
    }
}