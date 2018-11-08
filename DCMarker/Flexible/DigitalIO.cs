﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Contracts;
using LaserWrapper;

namespace DCMarker.Flexible
{
    internal class DigitalIO
    {
        private readonly DCConfig cfg;
        private Laser _laserWrapper;
        private readonly IoSignals sig;

        public DigitalIO(Laser laserWrapper)
        {
            _laserWrapper = laserWrapper;
            cfg = DCConfig.Instance;
            if (!File.Exists(cfg.ConfigName))
            {
                //RaiseErrorEvent("Config file is not found! dcmarker.xml in program directory");
            }
            sig = IoSignals.Instance;
            //UpdateIoMasks();
        }

        public bool SetArticleReady()
        {
            return _laserWrapper.SetPort(0, sig.MASK_ARTICLEREADY);
        }

        public bool ResetArticleReady()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_ARTICLEREADY);
        }

        public bool SetReady(bool mode)
        {
            return _laserWrapper.SetReady(mode);
        }

        public bool SetReadyToMark()
        {
            return _laserWrapper.SetPort(0, sig.MASK_READYTOMARK);
        }

        public bool ResetReadyToMark()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_READYTOMARK);
        }

        public bool SetHandleWithCare()
        {
            return _laserWrapper.SetPort(0, sig.MASK_HANDLEWITHCARE);
        }

        public bool ResetHandleWithCare()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_HANDLEWITHCARE);
        }

        public bool SetLastEdge()
        {
            return _laserWrapper.SetPort(0, sig.MASK_LASTEDGE);
        }

        public bool ResetLastEdge()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_LASTEDGE);
        }

        public bool SetMarkingDone()
        {
            return _laserWrapper.SetPort(0, sig.MASK_MARKINGDONE);
        }

        public bool ResetMarkingDone()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_MARKINGDONE);
        }

        public bool SetError()
        {
            return _laserWrapper.SetPort(0, sig.MASK_ERROR);
        }

        public bool ResetError()
        {
            return _laserWrapper.ResetPort(0, sig.MASK_ERROR);
        }

        private void UpdateIoMasks()
        {
            // Out

            sig.MASK_READYTOMARK = cfg.ReadyToMark;
            sig.MASK_NEXTTOLAST = cfg.NextToLast;
            sig.MASK_ARTICLEREADY = cfg.ArticleReady;
            sig.MASK_MARKINGDONE = cfg.MarkingDone;
            sig.MASK_LASTEDGE = cfg.LastEdge;
            sig.MASK_HANDLEWITHCARE = cfg.HandleWithCare;
            sig.MASK_ERROR = cfg.Error;
            sig.MASK_ALL = sig.MASK_ARTICLEREADY | sig.MASK_READYTOMARK | sig.MASK_NEXTTOLAST | sig.MASK_MARKINGDONE | sig.MASK_LASTEDGE | sig.MASK_HANDLEWITHCARE | sig.MASK_ERROR;

            // In
            sig.MASK_ITEMINPLACE = cfg.ItemInPlace;
            sig.MASK_EMERGENCY = cfg.EmergencyError;
            sig.MASK_RESET = cfg.ResetIo;
        }
    }
}