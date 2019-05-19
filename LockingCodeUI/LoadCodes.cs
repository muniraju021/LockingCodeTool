using log4net;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LockingCodeUI
{
    public partial class LoadCodes : Form
    {
        private ILoadLockingHandler _loadLockingHandler;
        private ILog _logger;
        private ISerialPortService _serialPortService;

        private List<string> _codeContentsSourceFile = new List<string>();
        private List<string> _codeContentsResultFile = new List<string>();
        private int _lineCount = 0;
        private string _archivedCodeFilePath;
        private string _sourceCodeFilePath;
        private string _sourceFileName;
        delegate void WriteAuditMessage(string message);
        WriteAuditMessage _delWriteMessage;

        public LoadCodes(ISerialPortService serialPortService, ILoadLockingHandler loadLockingHandler)
        {
            InitializeComponent();
            this._loadLockingHandler = loadLockingHandler;
            this._logger = LogManager.GetLogger(typeof(LoadCodes));
            this._serialPortService = serialPortService;
            InitialzieControls();
        }

        private void InitialzieControls()
        {
            var lstPorts = _serialPortService.GetSerialComPorts();
            cboComPorts.DataSource = lstPorts;
            _archivedCodeFilePath = ConfigurationManager.AppSettings["ArchiveCodeBytesPath"];
            _sourceCodeFilePath = ConfigurationManager.AppSettings["SourceCodeBytesPath"];

            CreateDirectory(_sourceCodeFilePath);
            CreateDirectory(_archivedCodeFilePath);

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
            _codeContentsResultFile = new List<string>();
            _codeContentsSourceFile = new List<string>();
        }
        
        private void btnLoad_Click(object sender, EventArgs e)
        {
            Reset();

            //Read Contents of file
            OpenFileDialog openFileDialog = new OpenFileDialog();            
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;
                _sourceFileName = Path.GetFileName(fileName);
                var comPort = cboComPorts.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(comPort) && !string.IsNullOrEmpty(fileName))
                {                    
                    _codeContentsSourceFile = File.ReadAllLines(fileName).ToList();
                    _serialPortService.InitializeSerialPortComm(comPort,callBackOnDataReceived: ProcessAcknowledgedMessage);
                    WriteToDevice();
                }
            }            
        }

        private void WriteToDevice()
        {
            _serialPortService.WriteToDevice<string>(_codeContentsSourceFile[_lineCount]);
        }

        

        private void SetTextMessage(string message)
        {
            txtLog.Text += message;
        }

        private void SetText(string message)
        {
            _delWriteMessage.Invoke(message);
        }

        private void ProcessAcknowledgedMessage(string message)
        {
            if(message == "R")
            {
                _codeContentsResultFile.Add(_codeContentsSourceFile[_lineCount]);
                _lineCount++;
                if (_lineCount < _codeContentsSourceFile.Count)
                {                    
                    _serialPortService.WriteToDevice<string>(_codeContentsSourceFile[_lineCount]);
                    SetText($"Loading in Progess... {_codeContentsSourceFile[_lineCount]} {Environment.NewLine}");
                }
                else
                {
                    if(_codeContentsSourceFile.Count == _codeContentsResultFile.Count)
                    {
                        var resultFilePath = Path.Combine(_archivedCodeFilePath, $"{_sourceFileName}_{DateTime.Now.ToString("ddMMyyyyhhmmss")}");
                        File.WriteAllLines(resultFilePath, _codeContentsResultFile.ToArray());
                        var sourceFilePath = Path.Combine(_sourceCodeFilePath, $"{_sourceFileName}");
                        File.WriteAllLines(sourceFilePath, _codeContentsSourceFile.ToArray());
                        SetText($"Loading in Completed!!! Archived File: {resultFilePath} {Environment.NewLine}");
                        MessageBox.Show("Loading Completed..");
                        _serialPortService.CloseConnection();
                    }
                }
            }
        }
    }
}
