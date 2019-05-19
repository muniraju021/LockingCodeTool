using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity;

namespace LockingCodeUI
{
    public partial class MainPageControl : Form
    {
        private IUnityContainer _container;

        public MainPageControl(IUnityContainer container)
        {            
            InitializeComponent();
            this._container = container;
        }

        private void ShowLoadCodes(object sender, EventArgs e)
        {
            var loadCodes = (LoadCodes)_container.Resolve(typeof(LoadCodes));
            loadCodes.ShowDialog();
        }

        private void ShowVerifyCodes(object sender, EventArgs e)
        {
            var verifyCodes = (VerifyCodes)_container.Resolve(typeof(VerifyCodes));
            verifyCodes.ShowDialog();
        }
                
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
