using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;

namespace CommunicationService
{
    public partial class CommService
    {
        #region Article Event

        public delegate void ArticleHandler(ArticleData data);

        public event EventHandler<ArticleDataArgs> ArticleEvent;

        internal void RaiseArticleEvent(ArticleData data)
        {
            var handler = ArticleEvent;
            if (handler != null)
            {
                var arg = new ArticleDataArgs(data);
                handler(null, arg);
            }
        }

        #endregion Article Event

        #region Laser End Event

        public delegate void LaserEndHandler();

        public event EventHandler<EventArgs> LaserEndEvent;

        internal void RaiseLaserEndEvent()
        {
            var handler = LaserEndEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Laser End Event

        #region Item in place Event

        public delegate void ItemInPlaceHandler();

        public event EventHandler<EventArgs> ItemInPlaceEvent;

        internal void RaiseItemInPlaceEvent()
        {
            var handler = ItemInPlaceEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Item in place Event

        #region Start Marking Event

        public delegate void StartMarkingHandler();

        public event EventHandler<EventArgs> StartMarkingEvent;

        internal void RaiseStartMarkingEvent()
        {
            var handler = StartMarkingEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Start Marking Event

        #region Restart application Event

        public delegate void RestartHandler();

        public event EventHandler<EventArgs> RestartEvent;

        internal void RaiseRestartEvent()
        {
            var handler = RestartEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Restart application Event
    }
}