using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortComLib
{
    public class LoggingManager
    {        
        static LoggingManager()
        {            
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));            
        }

        public static ILog GetLogger(Type type)
        {
            return log4net.LogManager.GetLogger(type);
        }
    }
}
