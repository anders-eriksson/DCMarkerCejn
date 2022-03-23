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
        public void _ioPort_sigInputChange(int p_nPort, int p_nBits)
        {
            Log.Trace(string.Format("Bit: {0}", p_nBits));

            // Item In Place
            if (sig.MASK_ITEMINPLACE != 0)
            {
                if ((p_nBits & sig.MASK_ITEMINPLACE) == sig.MASK_ITEMINPLACE)
                {
                    Log.Debug("MASK_ITEMINPLACE");
                    // bit is set
                    Log.Trace(string.Format("(currentBits & MASK_ITEMINPLACE): {0}", (currentBits & sig.MASK_ITEMINPLACE)));
                    if ((currentBits & sig.MASK_ITEMINPLACE) != sig.MASK_ITEMINPLACE)
                    {
                        Log.Trace("Set ITEMINPLACE ");
                        currentBits |= sig.MASK_ITEMINPLACE;
                        RaiseItemInPositionEvent();
                    }
                    else
                    {
                        Log.Trace(string.Format("Already in currentBits {0}", currentBits));
                    }
                }
                else
                {
                    Log.Trace("Reset ITEMINPLACE ");
                    currentBits &= ~sig.MASK_ITEMINPLACE;
                }
            }

            // EWxternal Start
            if (sig.MASK_EXTERNALSTART != 0)
            {
                if ((p_nBits & sig.MASK_EXTERNALSTART) == sig.MASK_EXTERNALSTART)
                {
                    Log.Debug("MASK_EXTERNALSTART");
                    // bit is set
                    Log.Trace(string.Format("(currentBits & MASK_EXTERNALSTART): {0}", (currentBits & sig.MASK_EXTERNALSTART)));
                    if ((currentBits & sig.MASK_EXTERNALSTART) != sig.MASK_EXTERNALSTART)
                    {
                        Log.Trace("Set EXTERNALSTART ");
                        currentBits |= sig.MASK_EXTERNALSTART;
                        //RaiseExternalStartEvent();
                        _laserSystem_sigQueryStart();
                    }
                    else
                    {
                        Log.Trace(string.Format("Already in currentBits {0}", currentBits));
                    }
                }
                else
                {
                    Log.Trace("Reset MASK_EXTERNALSTART ");
                    currentBits &= ~sig.MASK_EXTERNALSTART;
                }
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

        #region External Start Event

        public delegate void ExternalStartHandler();

        public event ExternalStartHandler ExternalStartEvent;

        internal void RaiseExternalStartEvent()
        {
            var handler = ExternalStartEvent;
            if (handler != null)
            {
                handler();
            }
        }


        public class ExternalStartArgs : EventArgs
        {
            public ExternalStartArgs(string msg)
            {
                Text = msg;
            }

            public string Text { get; private set; }
        }

        #endregion External Start Event


    }
}