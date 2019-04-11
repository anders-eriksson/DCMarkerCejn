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
            MASK_ARTICLEREADY = cfg.ArticleReady;
            NameDict.Add(MASK_ARTICLEREADY, "MASK_ARTICLEREADY");

            MASK_READYTOMARK = cfg.ReadyToMark;
            NameDict.Add(MASK_READYTOMARK, "MASK_READYTOMARK");

            MASK_MARKINGDONE = cfg.MarkingDone;
            NameDict.Add(MASK_MARKINGDONE, "MASK_MARKINGDONE");

            MASK_NEXTTOLAST = cfg.NextToLast;
            NameDict.Add(MASK_NEXTTOLAST, "MASK_NEXTTOLAST");

            MASK_LASTEDGE = cfg.LastEdge;
            NameDict.Add(MASK_LASTEDGE, "MASK_LASTEDGE ");

            MASK_HANDLEWITHCARE = cfg.HandleWithCare;
            NameDict.Add(MASK_HANDLEWITHCARE, "MASK_HANDLEWITHCARE");

            MASK_ERROR = cfg.Error;
            NameDict.Add(MASK_ERROR, "MASK_ERROR");

            MASK_ALL = 0Xffff;
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

        public int MASK_READYTOMARK = 0x01;
        public int MASK_MARKINGDONE = 0x02;
        public int MASK_ARTICLEREADY = 0x10;
        public int MASK_NEXTTOLAST = 0x40;
        public int MASK_LASTEDGE = 0x04;
        public int MASK_HANDLEWITHCARE = 0x20;
        public int MASK_ERROR = 0x80;
        public int MASK_ALL = 0Xffff;

        // In signals

        public int MASK_ITEMINPLACE=0x02;
        public int MASK_EMERGENCY=0x10 ;

        public int MASK_RESET=0x80;

        #endregion Laser IO
    }
}