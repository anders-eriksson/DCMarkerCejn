using Configuration;
using Contracts;
using DCLog;
using laserengineLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using GlblRes = global::LaserWrapper.Properties.Resources;

namespace LaserWrapper
{
    public partial class Laser : ILaser, IDigitalIo, Contracts.IAxis
    {
        private void _ioPort_sigInputChange(int p_nPort, int p_nBits)
        {
            Log.Trace(string.Format("Bit: {0}", p_nBits));
            // Item In Place
            if ((p_nBits & sig.MASK_ITEMINPLACE) == sig.MASK_ITEMINPLACE)
            {
                Log.Debug("MASK_ITEMINPLACE");
                // bit is set
                if ((currentBits & sig.MASK_ITEMINPLACE) != sig.MASK_ITEMINPLACE)
                {
                    Log.Trace("Set ITEMINPLACE ");
                    currentBits |= sig.MASK_ITEMINPLACE;
                    RaiseItemInPositionEvent();
                }
            }
            else
            {
                Log.Trace("Reset ITEMINPLACE ");
                currentBits &= ~sig.MASK_ITEMINPLACE;
            }
#if false
            // Reset IO
            if ((p_nBits & sig.MASK_RESET) == sig.MASK_RESET)
            {
                Log.Debug("MASK_RESET");
                // bit is set
                if ((currentBits & sig.MASK_RESET) != sig.MASK_RESET)
                {
                    currentBits |= sig.MASK_RESET;
                    RaiseResetIoEvent();
                }
            }
            else
            {
                currentBits &= ~sig.MASK_RESET;
            }
#endif
        }

        #region Item in Position Event

        public delegate void ItemInPositionHandler();

        public event ItemInPositionHandler ItemInPositionEvent;

        internal void RaiseItemInPositionEvent()
        {
            ItemInPositionHandler handler = ItemInPositionEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion Item in Position Event
    }
}