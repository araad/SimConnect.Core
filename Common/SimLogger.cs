using System;
using System.IO;
using log4net;
using log4net.Appender;

namespace SIM.Connect.Common
{
    public class SimLogger
    {
        private static volatile bool mReady = false;
        protected static readonly ILog sLog = LogManager.GetLogger("SimLogger");
        private static object locker = new object();

        static public string LogDirectory
        {
            get
            {
                var wRootRepository = LogManager.GetRepository();
                foreach (var appender in wRootRepository.GetAppenders())
                {
                    if (appender is FileAppender)
                    {
                        var wFileAppender = appender as FileAppender;
                        string wPath = wFileAppender.File;

                        return new FileInfo(wPath).DirectoryName;
                    }
                }

                return string.Empty;
            }
        }

        static private ILog Instance
        {
            get
            {
                if (!mReady)
                {
                    lock (locker)
                    {
                        if (!mReady)
                        {
                            startLogger();
                            mReady = true;
                        }
                    }
                }

                return sLog;
            }
        }

        static private void startLogger()
        {
            log4net.Config.XmlConfigurator.Configure();

            var wRootRepository = LogManager.GetRepository();
            foreach (var appender in wRootRepository.GetAppenders())
            {
                if (appender is FileAppender)
                {
                    var wFileAppender = appender as FileAppender;
                    wFileAppender.File += string.Format("FSX_SimLogger_{0}_({1}-{2}-{3}).txt",
                        DateTime.Now.GetDateTimeFormats()[3], DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    wFileAppender.ActivateOptions();
                }
            }
        }

        static public void Log(LogMode iMode, string iProviderName, string iEventMessage, params string[] iExceptionMessages)
        {
            string wLogMessage = string.Format("{0}, {1}", iProviderName, iEventMessage);
            foreach (string wExcMsg in iExceptionMessages)
            {
                wLogMessage += string.Format(", {0}", wExcMsg);
            }

            switch (iMode)
            {
                case LogMode.Debug:
                    Instance.Debug(wLogMessage);
                    return;
                case LogMode.Error:
                    Instance.Error(wLogMessage);
                    return;
                case LogMode.Info:
                    Instance.Info(wLogMessage);
                    return;
                case LogMode.Warn:
                    Instance.Warn(wLogMessage);
                    return;
            }
        }
    }

    public enum LogMode
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
