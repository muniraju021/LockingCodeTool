using log4net;
using log4net.Core;
using SerialPortComLib.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortComLib.Handler
{
    public interface ILoadLockingHandler
    {
        void LoadCodes(string codeFile = null, int index = 0);
    }

    public class LoadLockingCodesHandler : ILoadLockingHandler
    {
        private ISerialPortService _serialPortService = null;
        private ILog _logger;
        private string SourceCodeBytesPath;
        private string ArchiveCodeBytesPath;

        private List<string> _codesColl;
        private List<string> _resultCodesColl;

        public LoadLockingCodesHandler(ISerialPortService serialPortService)
        {
            _serialPortService = serialPortService;
            _serialPortService.CallBackOnDataReceived = CallBackOnDataReceived;
            _logger = LogManager.GetLogger(typeof(LoadLockingCodesHandler));
            SourceCodeBytesPath = ConfigurationManager.AppSettings["SourceCodeBytesPath"];
            ArchiveCodeBytesPath = ConfigurationManager.AppSettings["ArchiveCodeBytesPath"];

            CreateDirectory(SourceCodeBytesPath);
            CreateDirectory(ArchiveCodeBytesPath);
        }

        private void CreateDirectory(string path)
        {
            if(!string.IsNullOrWhiteSpace(path))
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        private void CallBackOnDataReceived(string result)
        {
            if(!string.IsNullOrWhiteSpace(result))
            {
                _resultCodesColl.Add(result);
                if(result.Equals("OK"))
                {
                    LoadCodes(index: _resultCodesColl.Count - 1);
                }
            }
        }   


        public void LoadCodes(string codeFile = null,int index = 0)
        {
            try
            {
                if (_codesColl == null || _codesColl.Count == 0)
                {
                    _codesColl = File.ReadAllLines(codeFile).ToList();
                }
                if(_codesColl != null && _codesColl.Count > 0)
                {
                    if(index < _codesColl.Count)
                        _serialPortService.WriteToDevice(_codesColl[index]);
                    else
                    {
                        ArchiveCodeFile();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat($"Error in Loading Codes File: {codeFile} | Index : {index}");
                _logger.ErrorFormat($"Exception: {ex.Message} {ex.StackTrace}");
            }
            
            _serialPortService.WriteToDevice<string>("80");
        }

        public void VerifyCode(string codeFile)
        {

        }

        private void ArchiveCodeFile()
        {
            var archiveCodeFilePath = Path.Combine(ArchiveCodeBytesPath, $"Codes{DateTime.Now.ToString("DDMMYYYHHmmss")}");
            File.WriteAllLines(archiveCodeFilePath, _codesColl.ToArray());
        }
    }
}
