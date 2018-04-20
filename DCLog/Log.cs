using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;

namespace DCLog
{
    public class Log
    {
        /*  Log Levels
         *  The following are the allowed log levels (in descending order):
         *  Off
         *  Fatal
         *  Error
         *  Warn
         *  Info
         *  Debug
         *  Trace
         */

        //private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static Logger _logger = NLog.LogManager.GetLogger("dclogger");

        private static object logLock = new object();

        public static void Debug(string message)
        {
            dclog(LogLevel.Debug, message);
        }

        public static void Error(string message)
        {
            dclog(LogLevel.Error, message);
        }

        public static void Error(string message, Exception ex)
        {
            dclog(LogLevel.Error, message, ex);
        }

        public static void Error(Exception ex, string message)
        {
            dclog(LogLevel.Error, message, ex);
        }

        public static void Fatal(string message)
        {
            dclog(LogLevel.Fatal, message);
        }

        public static void Fatal(string message, Exception ex)
        {
            dclog(LogLevel.Fatal, message, ex);
        }

        public static void Fatal(Exception ex, string message)
        {
            dclog(LogLevel.Fatal, message, ex);
        }

        public static void Info(string message)
        {
            dclog(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            dclog(LogLevel.Warn, message);
        }

        public static void Trace(string message)
        {
            dclog(LogLevel.Trace, message);
        }

        public static string GetLogFileName(string targetName)
        {
            string fileName = null;

            if (LogManager.Configuration != null && LogManager.Configuration.ConfiguredNamedTargets.Count != 0)
            {
                Target target = LogManager.Configuration.FindTargetByName(targetName);
                if (target == null)
                {
                    throw new Exception("Could not find target named: " + targetName);
                }

                FileTarget fileTarget = null;
                WrapperTargetBase wrapperTarget = target as WrapperTargetBase;

                // Unwrap the target if necessary.
                if (wrapperTarget == null)
                {
                    fileTarget = target as FileTarget;
                }
                else
                {
                    fileTarget = wrapperTarget.WrappedTarget as FileTarget;
                }

                if (fileTarget == null)
                {
                    throw new Exception("Could not get a FileTarget from " + target.GetType());
                }

                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
                fileName = fileTarget.FileName.Render(logEventInfo);
            }
            else
            {
                throw new Exception("LogManager contains no Configuration or there are no named targets");
            }

            //if (!File.Exists(fileName))
            //{
            //    throw new Exception("File " + fileName + " does not exist");
            //}

            return fileName;
        }

        /// <summary>
        /// The actual logging to NLog
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        /// <param name="ex">Exception</param>
        private static void dclog(LogLevel level, string message, Exception ex = null)
        {
#if TESTx
            string stackmsg = "|";

            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                StackFrame sf = st.GetFrame(i);
                string tmp = string.Format("{0} - {1} | ", sf.GetMethod(), sf.GetFileLineNumber());
                stackmsg += tmp;
            }
            message += stackmsg;
#endif
#if false
            return;
#else

            //lock (logLock)
            {
                LogEventInfo logEvent = new LogEventInfo(level, _logger.Name, message);
                if (ex != null)
                {
                    logEvent.Exception = ex;
                }

                _logger.Log(typeof(Log), logEvent);
            }
#endif
        }
    }
}