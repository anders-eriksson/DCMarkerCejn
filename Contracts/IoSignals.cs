namespace Contracts
{
    public class IoSignals
    {
        #region Laser IO

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