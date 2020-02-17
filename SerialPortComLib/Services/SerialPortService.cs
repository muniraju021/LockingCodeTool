using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortComLib.Services
{
    public class SerialPortService : ISerialPortService,IDisposable
    {
        SerialPort _serialPort;
        private const int serialPortWriteTimeout = 10000;
        private const int serialPortReadTimeout = 10000;
        private ILog _iLogger;

        public Action<string> CallBackOnDataReceived { get; set; }
                
        public SerialPortService(ILog logger)
        {
            //_iLogger = LoggingManager.GetLogger(typeof(SerialPortService));
            _iLogger = logger;
        }

        public void InitializeSerialPortComm(string portName, int baudrate = 9600,
            Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One,
            Action<string> callBackOnDataReceived = null)
        {
            _iLogger.InfoFormat($"SerialPortService: InitializeSerialPortComm - Serial Port Comm Started");
            if (_serialPort == null)
            {
                _serialPort = new SerialPort(portName, baudrate, parity, dataBits, stopBits);
                _serialPort.Handshake = Handshake.None;
                _serialPort.WriteTimeout = serialPortWriteTimeout;
                _serialPort.ReadTimeout = serialPortReadTimeout;
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.Open();

                _iLogger.InfoFormat($"SerialPortService: InitializeSerialPortComm - Serial Port - {portName} Opened");
            }

            this.CallBackOnDataReceived = callBackOnDataReceived;

            _iLogger.InfoFormat($"SerialPortService: InitializeSerialPortComm - Serial Port Comm Completed");

        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(sender is SerialPort)
            {
                if (_serialPort.IsOpen)
                {                    
                    var bytesToRead = _serialPort.BytesToRead;
                    var buffer = new byte[bytesToRead];

                    //var buffer = new List<byte>();
                    //var bytetemp = port.Read();
                    //while(Convert.ToInt32(bytetemp) != -1)
                    //{
                    //    buffer.Add(Convert.ToByte(bytetemp));
                    //    bytetemp = port.ReadByte();
                    //}

                    var resultbytes = _serialPort.Read(buffer, 0, bytesToRead);
                    var result = Encoding.UTF8.GetString(buffer.ToArray());
                    _iLogger.Info($"String Read: {result}");                    
                    CallBackOnDataReceived(result);
                }            
            }
        }
                        
        public bool WriteToDevice<T>(T dataToWrite)
        {
            if(!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            if (_serialPort != null && _serialPort.IsOpen)
            {
                if (typeof(T).Equals(typeof(string)))
                {
                    //_serialPort.WriteLine(dataToWrite.ToString());
                    var bytes = Encoding.ASCII.GetBytes(dataToWrite.ToString());
                    _serialPort.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            return false;
        }

        public List<string> GetSerialComPorts()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public void Dispose()
        {
            this._serialPort.Close();
            this._serialPort.Dispose();
        }

        public void CloseConnection()
        {
            this._serialPort?.Close();
            this._serialPort?.Dispose();
            this._serialPort = null;
        }
    }
}
