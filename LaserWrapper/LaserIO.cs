﻿using Configuration;
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

            // Item In Place
            if (sig.MASK_EXTERNTESTRESULT != 0)
            {
                if ((p_nBits & sig.MASK_EXTERNTESTRESULT) == sig.MASK_EXTERNTESTRESULT)
                {
                    Log.Debug("MASK_EXTERNTESTRESULT");
                    // bit is set
                    Log.Trace(string.Format("(currentBits & MASK_EXTERNTESTRESULT): {0}", (currentBits & sig.MASK_EXTERNTESTRESULT)));
                    if ((currentBits & sig.MASK_EXTERNTESTRESULT) != sig.MASK_EXTERNTESTRESULT)
                    {
                        Log.Trace("Set EXTERNTESTRESULT ");
                        currentBits |= sig.MASK_EXTERNTESTRESULT;
                        RaiseExternTestEvent(true);
                    }
                    else
                    {
                        Log.Trace(string.Format("Already in currentBits {0}", currentBits));
                    }
                }
                else
                {
                    Log.Trace("Reset MASK_EXTERNTESTRESULT ");
                    currentBits &= ~sig.MASK_EXTERNTESTRESULT;
                    RaiseExternTestEvent(false);
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

        #region Extern Test Event

        public delegate void ExternTestHandler(bool result);

        public event EventHandler<ExternTestArgs> ExternTestEvent;

        internal void RaiseExternTestEvent(bool result)
        {
            var handler = ExternTestEvent;
            if (handler != null)
            {
                var arg = new ExternTestArgs(result);
                handler(null, arg);
            }
        }

        public class ExternTestArgs : EventArgs
        {
            public ExternTestArgs(bool result)
            {
                Result = result;
            }

            public bool Result { get; private set; }
        }

        #endregion Extern Test Event
    }
}