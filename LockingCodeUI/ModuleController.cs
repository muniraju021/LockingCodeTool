using log4net;
using SerialPortComLib;
using SerialPortComLib.Handler;
using SerialPortComLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace LockingCodeUI
{
    public class ModuleController
    {
        public IUnityContainer Container;
        private ILog _logger;

        public void InitializeContainerClasses()
        {
            _logger = LoggingManager.GetLogger(typeof(ModuleController));
            Container = new UnityContainer();
            Container.RegisterInstance(_logger);
            Container.RegisterType<ISerialPortService, SerialPortService>();            
            Container.RegisterType<ILoadLockingHandler, LoadLockingCodesHandler>();
            Container.RegisterType(typeof(LoadCodes));
            Container.RegisterType(typeof(VerifyCodes));
        }
    }
}
