using Configuration;
using DCLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Threading;
using System.Diagnostics;

namespace DCAdam
{
    public class LogTelegrams
    {
        private static Logger _adamlogger = NLog.LogManager.GetLogger("adamlogger");
        private string _filename;
        private object LockLog = new object();
        private string lastMessage;
        private volatile bool lastDirection;

        //private StreamWriter _sw;

        public LogTelegrams()
        {
            _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DCLasersystem", "DCMarker", "Logs", "AdamTelegrams.log");
            //InitializeLogfile();
        }

        public LogTelegrams(string filename)
        {
            _filename = filename;
            //InitializeLogfile();
        }

        //private void InitializeLogfile()
        //{
        //    _sw = new StreamWriter(_filename);
        //    _sw.AutoFlush = true;
        //}

        public void WriteIn(string message)
        {
            int nTries = 5;
            do
            {
                try
                {
                    Write(true, message);
                    nTries = 0;
                }
                catch (IOException ex)
                {
                    nTries--;
                }
            } while (nTries > 0);
        }

        public void WriteOut(string message)
        {
            int nTries = 5;
            do
            {
                try
                {
                    Write(false, message);
                    nTries = 0;
                }
                catch (IOException ex)
                {
                    nTries--;
                }
            } while (nTries > 0);
        }

        public void Write(bool direction, string message)
        {
            try
            {
                if (DCConfig.Instance.AdamLogTelegrams)
                {
                    lock (LockLog)
                    {
                        if (!string.IsNullOrWhiteSpace(message) && (message != "0"))
                        {
                            lastDirection = direction;
                            lastMessage = message;
                            //using (StreamWriter w = new StreamWriter(_filename, true))
                            //{
                            //    _sw.WriteLine(string.Format("{0};{1};{2}", DateTime.Now.ToString("HH:mm:ss.fff"), direction ? "IN " : "OUT ", message));
                            //}
                            Debug.WriteLine(string.Format("{0}|{1}", direction ? "IN " : "OUT ", message));
                            _adamlogger.Info(string.Format("{0}|{1}", direction ? "IN " : "OUT ", message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("error writing to {0}", _filename));
                throw;
            }
        }
    }
}