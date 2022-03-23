using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Contracts;
using LaserWrapper;

namespace DCMarker.LargeFlexible
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
        }

        public bool SetArticleReady()
        {
            // av någon anledning blir ArticleReady signalen tvärt emot Hög blir låg, låg blir hög
            // Vi switchar så att SetArticleReady sätter signalen låg
            bool result = false;
            if (sig.MASK_ARTICLEREADY > 0)
            {
                DCLog.Log.Debug("Article Ready/OrderEjFärdig sätts Hög");
                result = _laserWrapper.ResetPort(0, sig.MASK_ARTICLEREADY);
            }
            return result;
        }

        public bool ResetArticleReady()
        {
            // av någon anledning blir ArticleReady signalen tvärt emot Hög blir låg, låg blir hög
            // Vi switchar så att ResetArticleReady sätter signalen hög
            bool result = false;
                if (sig.MASK_ARTICLEREADY > 0)
                {
                    DCLog.Log.Debug("Article Ready/OrderEjFärdig sätts Låg");
                    result = _laserWrapper.SetPort(0, sig.MASK_ARTICLEREADY);
                }
            return result;
        }

        public bool SetReady(bool mode)
        {
            return _laserWrapper.SetReady(mode);
        }

        public bool SetReadyToMark()
        {
            bool result = false;
            if (sig.MASK_READYTOMARK > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_READYTOMARK);
            return result;
        }

        public bool ResetReadyToMark()
        {
            bool result = false;
            if (sig.MASK_READYTOMARK > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_READYTOMARK);
            return result;
        }

        public bool SetHandleWithCare()
        {
            bool result = false;
            if (sig.MASK_HANDLEWITHCARE > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_HANDLEWITHCARE);
            return result;
        }

        public bool ResetHandleWithCare()
        {
            bool result = false;
            if (sig.MASK_HANDLEWITHCARE > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_HANDLEWITHCARE);
            return result;
        }

        public bool SetLastEdge()
        {
            bool result = false;
            if (sig.MASK_LASTEDGE > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_LASTEDGE);
            return result;
        }

        public bool ResetLastEdge()
        {
            bool result = false;
            if (sig.MASK_LASTEDGE > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_LASTEDGE);
            return result;
        }

        public bool SetMarkingDone()
        {
            bool result = false;
            if (sig.MASK_MARKINGDONE > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_MARKINGDONE);
            return result;
        }

        public bool ResetMarkingDone()
        {
            bool result = false;
            if (sig.MASK_MARKINGDONE > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_MARKINGDONE);
            return result;
        }

        public bool SetExternTest()
        {
            bool result = false;
            if (sig.MASK_EXTERNTEST > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_EXTERNTEST);
            return result;
        }

        public bool ResetExternTest()
        {
            bool result = false;
            if (sig.MASK_EXTERNTEST > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_EXTERNTEST);
            return result;
        }

        public bool SetError()
        {
            bool result = false;
            if (sig.MASK_ERROR > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_ERROR);
            return result;
        }

        public bool ResetError()
        {
            bool result = false;
            if (sig.MASK_ERROR > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_ERROR);
            return result;
        }

        public bool SetAll()
        {
            bool result = false;
            if (sig.MASK_ALL > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_ALL);
            return result;
        }

        public bool ResetAll()
        {
            bool result = false;
            if (sig.MASK_ALL > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_ALL);
            return result;
        }

        public bool SetNextToLast()
        {
            bool result = false;
            if (sig.MASK_NEXTTOLAST > 0)
                result = _laserWrapper.SetPort(0, sig.MASK_NEXTTOLAST);
            return result;
        }

        public bool ResetNextToLast()
        {
            bool result = false;
            if (sig.MASK_NEXTTOLAST > 0)
                result = _laserWrapper.ResetPort(0, sig.MASK_NEXTTOLAST);
            return result;
        }
    }
}