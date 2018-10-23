using Contracts;

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
        public Article Article { get; set; }
        public FlexibleItemStates ItemState { get; set; }

        public FlexibleItem()
        {
            ItemId = 0;
            CurrentEdge = 0;
            NumberOfEdges = 0;
            Article = new Article();
            ItemState = FlexibleItemStates.None;
        }

        public void Set(int itemid, int numberofedges, int currentedge, Article article, FlexibleItemStates state)
        {
            ItemId = itemid;
            NumberOfEdges = numberofedges;
            CurrentEdge = currentedge;
            Article = article;
            ItemState = state;
        }
    }
}