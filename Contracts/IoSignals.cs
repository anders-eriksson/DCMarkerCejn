using Configuration;
using System.Collections.Generic;

namespace Contracts
{
    public class IoSignals
    {
        #region Laser IO

        private static readonly object mutex = new object();
        private static volatile IoSignals instance;

        /// <summary>
        /// Constructor
        /// </summary>
        private IoSignals()
        {
            DCConfig cfg = DCConfig.Instance;
            MASK_ALL = 0;

            MASK_ARTICLEREADY = cfg.ArticleReady;
            if (MASK_ARTICLEREADY != 0)
            {
                MASK_ALL |= MASK_ARTICLEREADY;
                NameDict.Add(MASK_ARTICLEREADY, "MASK_ARTICLEREADY");
            }

            MASK_READYTOMARK = cfg.ReadyToMark;
            if (MASK_READYTOMARK != 0)
            {
                MASK_ALL |= MASK_READYTOMARK;
                NameDict.Add(MASK_READYTOMARK, "MASK_READYTOMARK");
            }

            MASK_MARKINGDONE = cfg.MarkingDone;
            if (MASK_MARKINGDONE != 0)
            {
                MASK_ALL |= MASK_MARKINGDONE;
                NameDict.Add(MASK_MARKINGDONE, "MASK_MARKINGDONE");
            }

            MASK_NEXTTOLAST = cfg.NextToLast;
            if (MASK_NEXTTOLAST != 0)
            {
                MASK_ALL |= MASK_NEXTTOLAST;
                NameDict.Add(MASK_NEXTTOLAST, "MASK_NEXTTOLAST");
            }

            MASK_LASTEDGE = cfg.LastEdge;
            if (MASK_LASTEDGE != 0)
            {
                MASK_ALL |= MASK_LASTEDGE;
                NameDict.Add(MASK_LASTEDGE, "MASK_LASTEDGE ");
            }

            MASK_HANDLEWITHCARE = cfg.HandleWithCare;
            if (MASK_HANDLEWITHCARE != 0)
            {
                MASK_ALL |= MASK_HANDLEWITHCARE;
                NameDict.Add(MASK_HANDLEWITHCARE, "MASK_HANDLEWITHCARE");
            }
            MASK_EXTERNTEST = cfg.ExternTest;
            if (MASK_EXTERNTEST != 0)
            {
                MASK_ALL |= MASK_EXTERNTEST;
                NameDict.Add(MASK_EXTERNTEST, "MASK_EXTERNTEST");
            }

            MASK_ERROR = cfg.Error;
            if (MASK_ERROR != 0)
            {
                MASK_ALL |= MASK_ERROR;
                NameDict.Add(MASK_ERROR, "MASK_ERROR");
            }
        }

        /// <summary>
        /// Instanciate the one and only object!
        /// </summary>
        public static IoSignals Instance
        {
            get
            {
                if (instance == null)

                {
                    lock (mutex)
                    {
                        if (instance == null)
                        {
                            // Call constructor
                            instance = new IoSignals();
                        }
                    }
                }
                return instance;
            }
        }

        public Dictionary<int, string> NameDict = new Dictionary<int, string>();

        // Out signals

        public int MASK_READYTOMARK = 0x0;
        public int MASK_MARKINGDONE = 0x0;
        public int MASK_ARTICLEREADY = 0x0;
        public int MASK_NEXTTOLAST = 0x0;
        public int MASK_LASTEDGE = 0x0;
        public int MASK_HANDLEWITHCARE = 0x0;
        public int MASK_ERROR = 0x0;
        public int MASK_EXTERNTEST = 0x0;
        public int MASK_ALL = 0Xffff;

        // In signals

        public int MASK_ITEMINPLACE = 0x0;
        public int MASK_EMERGENCY = 0x0;

        public int MASK_RESET = 0x0;

        #endregion Laser IO
    }
}