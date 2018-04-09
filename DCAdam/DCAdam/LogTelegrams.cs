using Configuration;
using DCLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdam
{
    public class LogTelegrams
    {
        private string _filename;
        private object LockLog = new object();
        private string lastMessage;

        public LogTelegrams()
        {
            _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DCLasersystem", "DCMarker", "Logs", "AdamTelegrams.log");
        }

        public LogTelegrams(string filename)
        {
            _filename = filename;
        }

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
                        if (message != lastMessage)
                        {
                            lastMessage = message;
                            using (StreamWriter w = new StreamWriter(_filename, true))
                            {
                                w.WriteLine(string.Format("{0};{1};{2}", DateTime.Now.ToString("HH:mm:ss.fff"), direction ? "IN " : "OUT ", message));
                            }
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