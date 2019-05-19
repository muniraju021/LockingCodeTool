using log4net;
using SerialPortComLib;
using SerialPortComLib.Handler;
using SerialPortComLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LockingCodeUI
{
    public partial class VerifyCodes : Form
    {
        private ILoadLockingHandler _loadLockingHandler;
        private ILog _logger;
        private ISerialPortService _serialPortService;
                
        private int _lineCount = 0;
        private List<string> _fileContents = new List<string>();
       
        delegate void WriteAuditMessage(string message);
        WriteAuditMessage _delWriteMessage;

        private const string VerifyMessageCode = "s";
        private ILog _iLogger;
        private int pgMultiples = 0;

        public VerifyCodes(ISerialPortService serialPortService, ILoadLockingHandler loadLockingHandler)
        {
            InitializeComponent();
            this._loadLockingHandler = loadLockingHandler;
            this._logger = LogManager.GetLogger(typeof(LoadCodes));
            this._serialPortService = serialPortService;
            _iLogger = LoggingManager.GetLogger(typeof(VerifyCodes));
            InitialzieControls();
        }

        private void InitialzieControls()
        {
            var lstPorts = _serialPortService.GetSerialComPorts();
            cboComPorts.DataSource = lstPorts;
                        
            _delWriteMessage = new WriteAuditMessage(SetTextMessage);
        }

        private void CreateDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();            
        }

        private void Reset()
        {
            _lineCount = 0;
            pgBar.Value = 0;
            pgBar.Visible = false;
            _fileContents = new List<string>();
            _serialPortService?.CloseConnection();
        }
        
        private void SetTextMessage(string message)
        {
            //txtLog.Text += message;
            _logger.Info($"{message}");
        }

        private void SetText(string message)
        {
            _delWriteMessage.Invoke(message);
        }
                
        private void btnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                Reset();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if(openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var fileName = openFileDialog.FileName;
                    _fileContents = File.ReadAllLines(fileName).ToList();
                    if(_fileContents != null && _fileContents.Count > 0)
                    {
                        pgBar.Visible = true;
                        _serialPortService.InitializeSerialPortComm(cboComPorts.SelectedValue.ToString(),callBackOnDataReceived:ProcessReceivedMessages);                        
                        WriteToDevice(VerifyMessageCode);
                        pgBar.Value += pgMultiples;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WriteToDevice(string message)
        {
            _serialPortService.WriteToDevice<string>(message);
        }

        StringBuilder truncatedMessage = new StringBuilder();
        private void ProcessReceivedMessages(string message)
        {
            if(!string.IsNullOrWhiteSpace(message))
            {
                _iLogger.Info($"Bytes Read: {message}");
                truncatedMessage.Append(message);
                                           
                if (truncatedMessage.ToString().EndsWith("R"))
                {                    
                    _iLogger.Info($"Bytes Processing: {truncatedMessage.ToString()}");
                    if (ProcessMessage(truncatedMessage.ToString(), _lineCount))
                    {
                        if(pgBar.Value < (pgBar.Maximum - 10))
                            pgBar.Value += 10;
                        truncatedMessage.Length = 0;
                        _lineCount++;
                        if (_lineCount < _fileContents.Count)                        
                            WriteToDevice(VerifyMessageCode);
                        else
                        {
                            pgBar.Value = pgBar.Maximum;
                            _logger.Info("Verification Successfull");
                            pgBar.Value += pgMultiples;
                            MessageBox.Show("Verification Successfull", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Reset();
                        }

                    }
                    else
                    {
                        _logger.WarnFormat($"Verification Failed at line {_lineCount + 1}");
                        MessageBox.Show($"Verification Failed at line {_lineCount}", "Fail",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        truncatedMessage.Length = 0;
                        Reset();
                    }
                    
                }               
            }
        }

        public bool ProcessMessage(string message,int index)
        {
            if(message.Contains(_fileContents[index]))
            {
                return true;
            }
            return false;
        }

        
    }
}
