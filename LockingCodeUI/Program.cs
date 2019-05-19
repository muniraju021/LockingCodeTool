using SerialPortComLib;
using SerialPortComLib.Handler;
using SerialPortComLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity;

namespace LockingCodeUI
{
    static class Program
    {
        

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var _iLogger = LoggingManager.GetLogger(typeof(Program));
            _iLogger.Info($"LockingCodeUI Starting..");
            ModuleController cntrl = new ModuleController();
            cntrl.InitializeContainerClasses();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainPageControl(cntrl.Container));
            _iLogger.Info($"LockingCodeUI Started..");
        }
    }
}
