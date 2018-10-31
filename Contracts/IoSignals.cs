using Configuration;

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
            MASK_READYTOMARK = cfg.ReadyToMark;
            MASK_MARKINGDONE = cfg.MarkingDone;
            MASK_NEXTTOLAST = cfg.NextToLast;
            MASK_LASTEDGE = cfg.LastEdge;
            MASK_HANDLEWITHCARE = cfg.HandleWithCare;
            MASK_ERROR = cfg.Error;
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

        public int MASK_ITEMINPLACE = 0x2;
        public int MASK_EMERGENCY = 0x10;

        public int MASK_RESET = 0x8;

        #endregion Laser IO
    }
}