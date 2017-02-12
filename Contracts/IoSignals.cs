﻿namespace Contracts
{
    public class IoSignals
    {
        #region Laser IO

        // Out signals

        public int MASK_READYTOMARK = 0x01;
        public int MASK_MARKINGDONE = 0x02;
        public int MASK_ERROR = 0x10;

        // In signals

        public int MASK_ITEMINPLACE = 0x2;
        public int MASK_EMERGENCY = 0x10;

        #endregion Laser IO
    }
}